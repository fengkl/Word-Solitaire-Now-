using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour {
    public static GameStateManager Instance { get; private set; }

    private const string SAVE_KEY = "CurrentGameState";

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else
            Destroy(gameObject);
    }

    /// <summary>
    /// 保存当前游戏状态
    /// </summary>
    public void SaveCurrentGameState() {
        GameState gameState = CapturedGameState();
        string json = JsonUtility.ToJson(gameState);

        // 保存到PlayerPrefs
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 捕获当前游戏状态
    /// </summary>
    /// <returns></returns>
    public GameState CapturedGameState() {
        GameState gameState = new GameState {
            levelData = GameDataUtils.Instance.levelData,
            gameMode = GameDataUtils.Instance.mode,
            levelId = GameDataUtils.Instance.levelId,
            randomSeed = GameDataUtils.Instance.levelData?.Seed ?? 0,
            gameTime = GameDataUtils.Instance.solitaireScene.topGroup.gameTimer.time,
            completeCount = GameDataUtils.Instance.completeCount,
            aimCount = GameDataUtils.Instance.aimCount,
            leftMoveCount = GameDataUtils.Instance.leftMoveCount,
            moveCount = GameDataUtils.Instance.moveCount
        };

        // 保存所有卡牌状态
        foreach (var kvp in GameDataUtils.Instance.cardActors) {
            CardActor card = kvp.Value;
            gameState.cardStates.Add(new CardState {
                cardName = card.cardName,
                cardCategory = card.cardCategory,
                cardType = card.cardType,
                isFaceDown = card.isFaceDown,
                canSelected = card.canSelected,
                position = card.transform.position,
                scale = card.transform.localScale,
                currentSlotId = card.currentSlot?.slotId.ToString() ?? ""
            });
        }

        // 保存所有槽位状态
        List<Slot> allSlots = GameDataUtils.Instance.GetAllSlots();
        foreach (var slot in allSlots) {
            SlotState slotState = new SlotState {
                slotId = slot.slotId.ToString(),
                slotType = slot.slotType,
                position = slot.transform.position,
                categoryCardName = (slot as TargetSlot)?.categoryCard?.cardName ?? ""
            };

            // 记录卡牌顺序
            foreach (var card in slot.cards)
                slotState.cardNames.Add(card.cardName);

            gameState.slotStates.Add(slotState);
        }

        // 单独保存翻牌卡槽状态
        HomeSlot homeSlot = GameDataUtils.Instance.solitaireScene.takeGroup.homeSlot;
        HomeSlotState homeSlotState = new HomeSlotState {
            countText = homeSlot.cards.Count,
            isShowCountText = homeSlot.counter.activeSelf
        };

        foreach (var card in homeSlot.cards)
            homeSlotState.cardNames.Add(card.cardName);
        homeSlotState.counterPosition = (homeSlot.counter.transform as RectTransform).anchoredPosition;

        gameState.homeSlotState = homeSlotState;

        // 保存当前关卡数据
        gameState.modeLevelData = new ModeLevelData {
            currentEasyLevelId = LevelData.levelIdDict[LevelMode.Easy],
            currentMediumLevelId = LevelData.levelIdDict[LevelMode.Normal],
            currentHardLevelId = LevelData.levelIdDict[LevelMode.Hard]
        };

        return gameState;
    }

    /// <summary>
    /// 检查是否存在已保存的残局
    /// </summary>
    /// <returns></returns>
    public bool HasSaveGameState() {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }

    /// <summary>
    /// 加载已保存的残局
    /// </summary>
    /// <returns></returns>
    public GameState LoadSavedGameState() {
        if (!HasSaveGameState()) return null;

        string json = PlayerPrefs.GetString(SAVE_KEY);
        GameState gameState = JsonUtility.FromJson<GameState>(json);

        // 恢复基本信息
        GameDataUtils.Instance.levelData = gameState.levelData;
        GameDataUtils.Instance.mode = gameState.gameMode;
        GameDataUtils.Instance.levelId = gameState.levelId;
        GameDataUtils.Instance.moveCount = gameState.moveCount;

        // 加载关卡数据
        GameDataUtils.Instance.LoadLevel(gameState.gameMode, gameState.randomSeed, true);

        Debug.Log($"残局加载成功 - 模式:{gameState.gameMode}, 关卡:{gameState.levelId}");
        return gameState;
    }


    /// <summary>
    /// 恢复游戏状态
    /// </summary>
    /// <param name="gameState"></param>
    public void RestoreGameState(GameState gameState) {
        if (gameState == null) return;

        // 恢复卡牌状态
        foreach (var cardState in gameState.cardStates) {
            if (GameDataUtils.Instance.cardActors.ContainsKey(cardState.cardName)) {
                CardActor card = GameDataUtils.Instance.cardActors[cardState.cardName];

                // 恢复卡牌属性
                card.transform.position = cardState.position;
                card.transform.localScale = cardState.scale;
                card.canSelected = cardState.canSelected;
                card.currentSlot = FindSlotById(cardState.currentSlotId);

                if (!cardState.isFaceDown)
                    card.FlipCard(false);
            }
        }

        // 恢复槽位状态
        RebuildSlotStates(gameState.slotStates);

        // 恢复翻牌卡槽的计数器位置
        GameDataUtils.Instance.solitaireScene.takeGroup.homeSlot.countText.text = gameState.homeSlotState.countText.ToString();
        GameDataUtils.Instance.solitaireScene.takeGroup.homeSlot.counter.SetActive(gameState.homeSlotState.isShowCountText);
        
        // 根据保存的卡牌顺序向翻牌卡槽添加卡牌
        for (int i = gameState.homeSlotState.cardNames.Count - 1; i >= 0; i--) {
            string cardName = gameState.homeSlotState.cardNames[i];
            if (GameDataUtils.Instance.cardActors.ContainsKey(cardName)) {
                CardActor card = GameDataUtils.Instance.cardActors[cardName];
                GameDataUtils.Instance.solitaireScene.takeGroup.homeSlot.AddCard(card);
            }
        }

        // 恢复游戏数据
        GameDataUtils.Instance.solitaireScene.topGroup.gameTimer.time = gameState.gameTime;
        GameDataUtils.Instance.aimCount = gameState.aimCount;
        GameDataUtils.Instance.completeCount = gameState.completeCount;
        GameDataUtils.Instance.solitaireScene.topGroup.gameProgressBar.SetProgress(gameState.completeCount, gameState.aimCount, false);
        GameDataUtils.Instance.solitaireScene.topGroup.dailyTarget.SetTarget(gameState.completeCount, gameState.aimCount);
        GameDataUtils.Instance.solitaireScene.topGroup.dailyMoves.InitMovesText(gameState.leftMoveCount);

        // 恢复完成进度计数
        GameDataUtils.Instance.completeCount = gameState.completeCount;
    }

    private void RebuildSlotStates(List<SlotState> slotStates) {
        // 清空槽位中的所有卡牌
        foreach (var slot in GameDataUtils.Instance.GetAllSlots())
            slot.cards.Clear();

        // 根据保存的状态重新分配卡牌
        foreach (var slotState in slotStates) {
            Slot targetSlot = FindSlotById(slotState.slotId);
            if (targetSlot == null) continue;

            // 恢复目标卡槽的分类卡
            if (targetSlot.slotType == SlotType.TargetSlot) {
                if (slotState.categoryCardName != "") {
                    CardActor card = GameDataUtils.Instance.cardActors[slotState.categoryCardName];
                    (targetSlot as TargetSlot).categoryCard = (CategoryCardActor)card;
                }
            }

            // 根据保存的卡牌顺序添加卡牌
            if (slotState.cardNames.Count > 0)
                targetSlot.LoadGame(slotState.cardNames);
        }
    }

    /// <summary>
    /// 根据id查找槽位
    /// </summary>
    private Slot FindSlotById(string slotId) {
        foreach (var slot in GameDataUtils.Instance.GetAllSlots()) {
            if (slot.slotId.ToString() == slotId)
                return slot;
        }
        return null;
    }

    /// <summary>
    /// 清除保存的残局数据
    /// </summary>
    public void ClearSavedGameState() {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
        print("已清除保存的残局数据");
    }

    /// <summary>
    /// 应用退出时保存
    /// </summary>
    private void OnApplicationQuit() {
        if (GameDataUtils.Instance.isGaming)
            SaveCurrentGameState();
    }
}
