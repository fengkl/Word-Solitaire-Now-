using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    
    private const string SAVE_KEY = "CurrentGameState";
    private const int MAX_SAVE_SLOTS = 3; // 最多保存3个残局

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 保存当前游戏状态
    /// </summary>
    public void SaveCurrentGameState()
    {
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
    public GameState CapturedGameState()
    {
        GameState gameState = new GameState
        {
            levelData = GameDataUtils.Instance.levelData,
            gameMode = GameDataUtils.Instance.mode,
            levelId = GameDataUtils.Instance.levelId,
            randomSeed = GameDataUtils.Instance.levelData?.Seed ?? 0,
            gameTime = GameDataUtils.Instance.solitaireScene.topGroup.gameTimer.time,
            completeCount = GameDataUtils.Instance.completeCount,
            aimCount = GameDataUtils.Instance.aimCount
        };

        // 保存所有卡牌状态
        foreach (var kvp in GameDataUtils.Instance.cardActors)
        {
            CardActor card = kvp.Value;
            gameState.cardStates.Add(new CardState
            {
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
        List<Slot_New> allSlots = GameDataUtils.Instance.GetAllSlots();
        foreach (var slot in allSlots)
        {
            SlotState slotState = new SlotState
            {
                slotId = slot.slotId.ToString(),
                slotType = slot.slotType,
                position = slot.transform.position
            };
            
            // 记录卡牌顺序
            foreach (var card in slot.cards)
            {
                slotState.cardNames.Add(card.cardName);
            }
            
            gameState.slotStates.Add(slotState);
        }

        return gameState;
    }

    /// <summary>
    /// 检查是否存在已保存的残局
    /// </summary>
    /// <returns></returns>
    public bool HasSaveGameState()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }
    
    /// <summary>
    /// 加载已保存的残局
    /// </summary>
    /// <returns></returns>
    public GameState LoadSavedGameState()
    {
        if (!HasSaveGameState()) return null;
        
        string json = PlayerPrefs.GetString(SAVE_KEY);
        GameState gameState = JsonUtility.FromJson<GameState>(json);
        
        // 恢复基本信息
        GameDataUtils.Instance.levelData = gameState.levelData;
        GameDataUtils.Instance.mode = gameState.gameMode;
        GameDataUtils.Instance.levelId = gameState.levelId;

        Debug.Log($"残局加载成功 - 模式:{gameState.gameMode}, 关卡:{gameState.levelId}");
        return gameState;
    }
    
    /// <summary>
    /// 恢复游戏状态
    /// </summary>
    /// <param name="gameState"></param>
    public void RestoreGameState(GameState gameState)
    {
        if (gameState == null) return;
    
        // 恢复卡牌状态
        foreach (var cardState in gameState.cardStates)
        {
            if (GameDataUtils.Instance.cardActors.ContainsKey(cardState.cardName))
            {
                CardActor card = GameDataUtils.Instance.cardActors[cardState.cardName];

                // 恢复卡牌属性
                card.isFaceDown = cardState.isFaceDown;
                card.transform.position = cardState.position;
                card.transform.localScale = cardState.scale;
            
                card.FlipCard(0);
                card.canSelected = cardState.canSelected;
            }
        }

        // 恢复槽位状态
        RebuildSlotStates(gameState.slotStates);

        if (GameDataUtils.Instance.solitaireScene.topGroup.gameTimer != null)
        {
            GameDataUtils.Instance.solitaireScene.topGroup.gameTimer.time = gameState.gameTime;
        }

        // 恢复完成进度计数
        GameDataUtils.Instance.completeCount = gameState.completeCount;

        print("残局恢复完成");
    }
    
    private void RebuildSlotStates(List<SlotState> slotStates)
    {
        // 清空槽位中的所有卡牌
        foreach (var slot in GameDataUtils.Instance.GetAllSlots())
        {
            slot.cards.Clear();
        }
        
        // 根据保存的状态重新分配卡牌
        foreach (var slotState in slotStates)
        {
            Slot_New targetSlot = FindSlotById(slotState.slotId);
            if (targetSlot == null) continue;
            
            // 根据保存的卡牌顺序添加卡牌
            for (int i = slotState.cardNames.Count - 1; i >= 0; i--)
            {
                string cardName = slotState.cardNames[i];
                if (GameDataUtils.Instance.cardActors.ContainsKey(cardName))
                {
                    CardActor card = GameDataUtils.Instance.cardActors[cardName];
                    targetSlot.AddCard(card);
                }
            }
        }

        // 更新卡槽中卡牌的位置
        foreach (var slot in GameDataUtils.Instance.GetAllSlots())
        {
            slot.UpdateCardsPos();
        }
    }
    
    /// <summary>
    /// 根据id查找槽位
    /// </summary>
    private Slot_New FindSlotById(string slotId)
    {
        foreach (var slot in GameDataUtils.Instance.GetAllSlots())
        {
            if (slot.slotId.ToString() == slotId)
            {
                return slot;
            }
        }
        return null;
    }
    
    /// <summary>
    /// 清除保存的残局数据
    /// </summary>
    public void ClearSavedGameState()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
        print("已清除保存的残局数据");
    }
    
    /// <summary>
    /// 应用退出时保存
    /// </summary>
    private void OnApplicationQuit()
    {
        if (GameDataUtils.Instance.levelId != 0)
        {
            SaveCurrentGameState();
        }
    }
}
