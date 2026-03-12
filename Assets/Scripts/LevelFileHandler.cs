using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelFileHandler {
    private readonly Dictionary<string, WordSotaLevelData> mLevels = new Dictionary<string, WordSotaLevelData>();
    private readonly Dictionary<string, WordSotaFixedLevelData> mFixedData = new Dictionary<string, WordSotaFixedLevelData>();
    private readonly LoopLevelFileHandler LoopHandler;

    private List<int> mDailyLevels = new List<int>();
    public void Init() {

        // 1. 加载资源
        // StringLiteral_15614 是文件名，需要在 global-metadata.dat 中查找具体字符串
        TextAsset textAsset = Resources.Load<TextAsset>("fixed");

        byte[] encryptedBytes = textAsset.bytes;

        // 2. 解密 (调用上一段分析的函数)
        byte[] decryptedBytes = Utils.DecryptAes(encryptedBytes);

        //3. 反序列化 (使用 MessagePack)
        //代码中暗示这是一个 List<WordSotaFixedLevelData>
        var dataList = MessagePackSerializer.Deserialize<List<WordSotaFixedLevelData>>(decryptedBytes);

        // 4. 遍历列表并填充字典
        foreach (var data in dataList) {
            // data.Field12 和 data.Field8 是 WordSotaFixedLevelData 的成员
            // 用来生成字典的 Key
            string key = this.GetLevelKey(data.Mode, data.LevelID);

            // Debug.Log(data);


            // 存入字典: levelDataDict[key] = data;
            this.mFixedData[key] = data;
        }

        // 5. 初始化下一个模块
        if (this.LoopHandler != null)
            this.LoopHandler.Init();

        //Debug.Log("done:" + dataList.Count);
    }

    public void LoadAllLevels() {
        // 1. 获取 LevelMode 枚举的所有值
        Array enumValues = Enum.GetValues(typeof(LevelMode));
        IEnumerator enumerator = enumValues.GetEnumerator();

        // 2. 遍历每一个模式
        while (enumerator.MoveNext()) {
            // Unbox: 将 object 强转回 LevelMode 枚举
            LevelMode currentMode = (LevelMode)enumerator.Current;

            // 3. 获取该模式对应的资源文件名
            string resName = this.ModeToResName(currentMode);

            // 5. 加载资源
            //TextAsset textAsset = AssetLoader.LoadByName<TextAsset>(resName);
            TextAsset textAsset = Resources.Load<TextAsset>(resName);


            if (textAsset != null) {
                // 6. 解密
                byte[] decryptedBytes = Utils.DecryptAes(textAsset.bytes);
                // 7. 反序列化
                // 注意：这里泛型类型是 WordSotaLevelData
                var levelList = MessagePackSerializer.Deserialize<List<WordSotaLevelData>>(decryptedBytes);

                Debug.Log("Mode=" + currentMode + " levelCount=" + levelList.Count);

                // 8. 处理加载的数据 (存储到内存)
                this.HandleLevels(currentMode, levelList);
            }
        }

        // 9. 调用下一个 Handler 的 LoadAllLevels
        if (this.LoopHandler != null)
            this.LoopHandler.LoadAllLevels();
    }
    public void HandleLevels(LevelMode mode, List<WordSotaLevelData> dataList) {
        if (dataList == null) return;

        // C++ 代码中将两种情况分成了两个大的 if-else 块，
        // 这里为了还原结构，也保持分开写的形式。

        if (mode == LevelMode.Daily) {
            foreach (WordSotaLevelData levelData in dataList) {
                // 1. 生成 Key (使用 mode 4 和 levelData.ID)
                // levelData.Field8 应该是 Level ID
                string key = this.GetLevelKey(mode, levelData.LevelID);

                // Debug.Log(key + " " + levelData);
                // 2. 存入主字典
                this.mLevels[key] = levelData;

                // 3. 特殊处理：添加到 ID 列表
                // 对应代码中的 v14 及其 Add 操作
                this.mDailyLevels.Add(levelData.LevelID);
            }
        } else {
            foreach (WordSotaLevelData levelData in dataList) {
                // 1. 生成 Key
                string key = this.GetLevelKey(mode, levelData.LevelID);

                Debug.Log(key + " " + levelData);
                // 2. 存入主字典
                this.mLevels[key] = levelData;
            }
        }
    }
    private string GetLevelKey(LevelMode mode, int levelID) {
        return mode.ToString() + levelID;
    }

    private string ModeToResName(LevelMode mode) {
        if (mode == LevelMode.Easy) return "easy";
        if (mode == LevelMode.Normal) return "normal";
        if (mode == LevelMode.Hard) return "hard";
        if (mode == LevelMode.Daily) return "daily";
        return null;
    }

    public bool TryGetLevelData(LevelMode mode, int levelId, int randomSeed, out GameplayLevelData outData) {
        // 1. 准备基础 Key 和 结构生成种子
        // GetLevelKey 内部逻辑推测是拼接字符串，例如 "Mode_LevelID"
        string cacheKey = this.GetLevelKey(mode, levelId);

        // 这个种子仅用于决定关卡的“结构”（有哪些词），保证同一关卡内容一致
        int structureSeed = GameplayUtils.CalculateRandomSeed((int)mode, levelId);

        WordSotaLevelData baseLevelData = null;

        // 2. 尝试从主缓存 (mLevels) 获取数据
        // a1[2] -> mLevels (Offset 0x8)
        if (!this.mLevels.TryGetValue(cacheKey, out baseLevelData)) {
            // --- 缓存未命中 (Miss) ---

            if (mode == LevelMode.Daily) { // Daily Mode (每日挑战)
                // 获取每日挑战存档处理器
                //var dailyHandler = AccountManager.Instance.GetHandler<DailyArchiveHandler>();

                // a1[5] -> mDailyLevels (Offset 0x14)
                List<int> dailyPool = this.mDailyLevels;

                // 计算循环索引：当前天数 % 每日关卡池总数
                // 汇编逻辑：v25 = v24 % list.Count
                int poolIndex = GamePreferences.DailyLevelIndex % dailyPool.Count;

                // 获取实际复用的关卡 ID
                int actualLevelId = dailyPool[poolIndex];

                // 生成实际 Key (注意：GetLevelKey 的第一个参数这里传了 actualLevelId，可能是前缀区分)
                string actualKey = this.GetLevelKey(mode, actualLevelId);

                // 从 mLevels 中获取预加载的每日关卡数据
                // 注意：Daily 关卡通常是预先打包在 mLevels 里的，而不是动态生成的
                baseLevelData = this.mLevels[actualKey];

                // 更新存档中的索引 (指向明天)
                //dailyHandler.SetLoopLevelIndex(poolIndex + 1);
                GamePreferences.DailyLevelIndex += 1;
            } else { // Loop Mode (无尽/普通随机模式)
                // a1[4] -> LoopHandler (Offset 0x10)
                // 动态生成新的关卡数据
                baseLevelData = this.LoopHandler.GenerateLevelData(mode, levelId, structureSeed);
            }
        }

        // 3. 处理固定数据覆盖 (Fixed Data)
        // a1[3] -> mFixedData (Offset 0xC)
        WordSotaFixedLevelData fixedData = null;

        // 生成最终的游戏运行时数据 (GameplayLevelData)
        // gameplaySeed (a4) 用于这一局的游戏内随机（如洗牌、掉落），与结构无关
        if (this.mFixedData.TryGetValue(cacheKey, out fixedData)) {
            // 如果有固定配置（例如教程引导），合并生成
            outData = WordSotaLevelGenerator.GenerateLevelData(baseLevelData, fixedData, randomSeed);
        } else {
            // 纯随机/普通生成
            outData = WordSotaLevelGenerator.GenerateLevelData(baseLevelData, randomSeed);
        }

        // 4. 设置元数据并返回
        if (outData != null) {
            outData.Mode = mode;
            outData.ID = levelId;
            outData.Seed = randomSeed;
        }

        return true;
    }

    public bool TryGetRandomLevelData(LevelMode mode, out GameplayLevelData data) {
        data = null;
        return true;
    }

    public LevelFileHandler() {
    }
}
