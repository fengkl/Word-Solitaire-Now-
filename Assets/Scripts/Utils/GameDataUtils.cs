using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 游戏数据工具类，用于管理游戏中的各种数据和组件
/// </summary>
public class GameDataUtils {
    public static GameDataUtils Instance { get; private set; } = new GameDataUtils();

    public SolitaireScene solitaireScene;

    public List<TargetSlot> targetSlots = new List<TargetSlot>();
    public List<OperateSlot> operateSlots = new List<OperateSlot>();
    public List<TakeSlot> takeSlots = new List<TakeSlot>();
    public Dictionary<string, CardActor> cardActors = new Dictionary<string, CardActor>();
    public Dictionary<string, Sprite> cardSpriteDict = new Dictionary<string, Sprite>();

    public LevelFileHandler levelHandler;
    public LoopLevelFileHandler loopLevelHandler;

    public GameplayLevelData levelData;
    public LevelMode mode;
    public int levelId = 0;
    public int aimCount; // 目标数量
    public int completeCount; // 完成数量
    public int leftMoveCount; // 剩余步数
    public int moveCount;

    public bool isGaming; // 是否在游戏中
    public bool isLoadGame; // 是否在加载中

    public GameState savedState; // 残局数据

    /// <summary>
    /// 加载关卡（测试）
    /// </summary>
    public void LoadTestLevel() {
        int levelId = 1; // 关卡id
        LevelMode mode = LevelMode.Easy; // 游戏难度
        int randomSeed = Random.Range(0, 9999); // 随机种子

        // 使用难度，id和种子来获取关卡数据
        if (levelHandler.TryGetLevelData(mode, levelId, randomSeed, out var levelData)) {
            Debug.Log($"成功加载关卡 {levelId}");
            Debug.Log(levelData.ToString());

            this.levelData = levelData;

            aimCount = levelData.KVPs.Count;
            moveCount = 0;
        }
    }

    /// <summary>
    /// 加载循环关卡（测试）
    /// </summary>
    public void LoadLoopTestLevel() {
        LevelMode mode = LevelMode.Easy; // 游戏难度
        int levelId = 1001; // 无尽模式关卡ID（只是标识用）
        int randomSeed = Random.Range(0, 9999); // 随机种子

        Debug.Log($"生成无尽模式关卡 - 模式: {mode}, 种子: {randomSeed}");

        // 直接使用LoopLevelFileHandler生成关卡数据
        WordSotaLevelData loopLevelData = loopLevelHandler.GenerateLevelData(mode, levelId, randomSeed);

        if (loopLevelData != null) {
            Debug.Log("无尽模式基础数据生成成功");

            // 使用WordSotaLevelGenerator转换为GameplayLevelData
            GameplayLevelData gameplayData = WordSotaLevelGenerator.GenerateLevelData(loopLevelData, randomSeed);

            // 补充必要字段
            gameplayData.Mode = mode;
            gameplayData.ID = levelId;
            gameplayData.Seed = randomSeed;

            Debug.Log($"成功生成无尽模式关卡 {levelId}");
            Debug.Log(gameplayData.ToString());
        } else
            Debug.LogError("无尽模式关卡生成失败");
    }

    /// <summary>
    /// 加载关卡
    /// </summary>
    public void LoadLevel(LevelMode mode, int randomSeed = -1, bool loadGame = false) {
        if (!loadGame)
        {
            if (mode == LevelMode.Daily)
                levelId = solitaireScene.calenderGroup.GetSelectDayLevelID();
            else
                levelId = LevelData.levelIdDict[mode];
        }
        
        this.mode = mode;
        randomSeed = randomSeed == -1 ? Random.Range(0, 9999) : randomSeed; // 随机种子

        // 使用难度，id和种子来获取关卡数据
        if (levelHandler.TryGetLevelData(mode, levelId, randomSeed, out var levelData)) {
            Debug.Log($"成功加载关卡 {levelId}");
            Debug.Log(levelData.ToString());

            this.levelData = levelData;

            aimCount = levelData.KVPs.Count;
        }
    }

    /// <summary>
    /// 获取所有卡槽
    /// </summary>
    public List<Slot> GetAllSlots() {
        List<Slot> allSlots = new List<Slot>();

        allSlots.AddRange(targetSlots);
        allSlots.AddRange(operateSlots);
        allSlots.AddRange(takeSlots);

        return allSlots;
    }
}
