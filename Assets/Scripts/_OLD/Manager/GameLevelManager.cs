using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLevelManager : MonoBehaviour
{
    public static GameLevelManager Instance { get; private set; }
    
    private LevelFileHandler levelHandler;
    private LoopLevelFileHandler loopLevelHandler;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        levelHandler = new LevelFileHandler();
        loopLevelHandler = new LoopLevelFileHandler();
        levelHandler.Init();
        loopLevelHandler.Init();
        levelHandler.LoadAllLevels();
        loopLevelHandler.LoadAllLevels();
        
        Debug.Log("关卡初始化完成");
    }
    
    void Start()
    {
        // LoadTestLevel();
        // LoadLoopTestLevel();
    }

    /// <summary>
    /// 加载关卡（测试）
    /// </summary>
    void LoadTestLevel()
    {
        int levelId = 1; // 关卡id
        LevelMode mode = LevelMode.Normal; // 游戏难度
        int randomSeed = Random.Range(0, 9999); // 随机种子

        GameplayLevelData levelData; // 游戏关卡数据

        // 使用难度，id和种子来获取关卡数据
        if (levelHandler.TryGetLevelData(mode, levelId, randomSeed, out levelData))
        {
            Debug.Log($"成功加载关卡 {levelId}");
            Debug.Log(levelData.ToString());
            
            GameManager.Instance.InitGame(levelData);
        }
    }
    
    /// <summary>
    /// 加载循环关卡（测试）
    /// </summary>
    void LoadLoopTestLevel()
    {
        LevelMode mode = LevelMode.Easy; // 游戏难度
        int levelId = 1001; // 无尽模式关卡ID（只是标识用）
        int randomSeed = Random.Range(0, 9999); // 随机种子
    
        Debug.Log($"生成无尽模式关卡 - 模式: {mode}, 种子: {randomSeed}");
    
        // 直接使用LoopLevelFileHandler生成关卡数据
        WordSotaLevelData loopLevelData = loopLevelHandler.GenerateLevelData(mode, levelId, randomSeed);
    
        if (loopLevelData != null)
        {
            Debug.Log("无尽模式基础数据生成成功");
        
            // 使用WordSotaLevelGenerator转换为GameplayLevelData
            GameplayLevelData gameplayData = WordSotaLevelGenerator.GenerateLevelData(loopLevelData, randomSeed);
        
            // 补充必要字段
            gameplayData.Mode = mode;
            gameplayData.ID = levelId;
            gameplayData.Seed = randomSeed;
        
            Debug.Log($"成功生成无尽模式关卡 {levelId}");
            Debug.Log(gameplayData.ToString());
        
            GameManager.Instance.InitGame(gameplayData);
        }
        else
        {
            Debug.LogError("无尽模式关卡生成失败");
        }
    }
    
    /// <summary>
    /// 加载关卡
    /// </summary>
    public void LoadLevel(LevelMode mode)
    {
        int levelId = LevelData.levelIdDict[mode];
        
        int randomSeed = Random.Range(0, 9999); // 随机种子

        GameplayLevelData levelData; // 游戏关卡数据

        // 使用难度，id和种子来获取关卡数据
        if (levelHandler.TryGetLevelData(mode, levelId, randomSeed, out levelData))
        {
            Debug.Log($"成功加载关卡 {levelId}");
            Debug.Log(levelData.ToString());
            
            GameManager.Instance.InitGame(levelData);
        }
    }
}
