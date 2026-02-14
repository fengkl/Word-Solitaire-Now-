using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LoopLevelFileHandler {
    private readonly Dictionary<string, List<string>> mCategoryDic = new Dictionary<string, List<string>>();

    private readonly HashSet<string> mWordCategorySet = new HashSet<string>();

    private readonly HashSet<string> mSpriteCategorySet = new HashSet<string>();

    private readonly Dictionary<LevelMode, List<WordSotaLoopLevelData>> mLoopLevels = new Dictionary<LevelMode, List<WordSotaLoopLevelData>>();

    private const int cMinWordsCnt = 3;

    private const int cMaxWordsCnt = 8;

    public void Init() {
        // 1. 加载资源 (StringLiteral_16735 需要查表确定具体文件名)
        TextAsset textAsset = Resources.Load<TextAsset>("loop_category");


        // 2. 解密
        byte[] decryptedBytes = Utils.DecryptAes(textAsset.bytes);

        // 3. 反序列化
        // 这里的 List<WordKVPInfo> 是根据代码推断的类型
        var dataList = MessagePackSerializer.Deserialize<List<WordKVPInfo>>(decryptedBytes);

        // 3. 遍历数据进行分类存储
        foreach (WordKVPInfo info in dataList) {
            // 将 "分类名" -> "单词列表" 存入主字典
            this.mCategoryDic[info.Category] = info.Words;

            // 根据 UseSprite 字段分流到不同的 HashSet
            if (info.UseSprite) {
                // 如果 UseSprite 为 true，存入图片分类集合 (Offset 16)
                this.mSpriteCategorySet.Add(info.Category);
            } else {
                // 如果 UseSprite 为 false，存入文本分类集合 (Offset 12)
                this.mWordCategorySet.Add(info.Category);
            }
        }
    }

    public void LoadAllLevels() {
        // 1. 遍历 LevelMode 枚举的所有值
        foreach (LevelMode mode in Enum.GetValues(typeof(LevelMode))) {
            // 2. 根据模式生成资源文件名
            string resName = this.ModeToResName(mode);

            //// 4. 检查资源是否存在，如果不存在则使用默认文件
            //if (!Resources.Exists(resName)) {
            //    // 回退机制
            //    resName = this.GetDefaultModeToResName(mode);
            //}

            // 5. 加载资源
            TextAsset textAsset = Resources.Load<TextAsset>(resName);

            if (textAsset != null) {
                // 6. 解密
                byte[] decryptedBytes = Utils.DecryptAes(textAsset.bytes);

                // 7. 反序列化
                var dataList = MessagePackSerializer.Deserialize<List<WordSotaLoopLevelData>>(decryptedBytes);

                // 8. 存入字典
                // this.loopLevelsDict.Add(mode, dataList);
                if (this.mLoopLevels != null)
                    this.mLoopLevels.Add(mode, dataList);
            }
        }
    }

    private string ModeToResName(LevelMode mode) {
        string prefix = "";
        if (mode == LevelMode.Easy) prefix= "loop_easy_";
        if (mode == LevelMode.Hard) prefix = "loop_hard_";
        if (mode == LevelMode.Normal) prefix = "loop_normal_";
        string suffix = "A";
        //if (PlayerPrefs.GetInt("")) suffix = "";
        return prefix + suffix;
    }
    private string GetDefaultModeToResName(LevelMode mode) {
        if (mode == LevelMode.Easy) return "loop_easy_A";
        if (mode == LevelMode.Hard) return "loop_hard_A";
        if (mode == LevelMode.Normal) return "loop_normal_A";
        return null;
    }
    public WordSotaLevelData GenerateLevelData(LevelMode mode, int levelId, int randomSeed) {
        // -------------------------------------------------------------------------
        // 1. 获取模板与确定目标
        // -------------------------------------------------------------------------
        List<WordSotaLoopLevelData> templates = this.mLoopLevels[mode];
        System.Random random = new System.Random(randomSeed);
        WordSotaLoopLevelData template = templates[random.Next(templates.Count)];

        // 计算本局目标
        int targetTotalWordCount = random.Next(template.MinWordCnt, template.MaxWordCnt + 1); // v133
        int targetSpriteCount = random.Next(template.MinUseSpriteCnt, template.MaxUseSpriteCnt + 1); // v128

        // -------------------------------------------------------------------------
        // 2. 准备候选词库
        // -------------------------------------------------------------------------
        List<string> wordCandidates = Utils.GetRandomList(new List<string>(mWordCategorySet), mWordCategorySet.Count, randomSeed); // v17 / v137
        List<string> spriteCandidates = Utils.GetRandomList(new List<string>(mSpriteCategorySet), mSpriteCategorySet.Count, randomSeed);

        // 排序
        wordCandidates.Sort();
        spriteCandidates.Sort();

        // -------------------------------------------------------------------------
        // 3. 生成分类 (Category Generation)
        // -------------------------------------------------------------------------
        // 使用字典方便后续查找，对应汇编中的 v142
        Dictionary<string, WordKVPInfo> generatedKvpDict = new Dictionary<string, WordKVPInfo>();

        HashSet<string> usedCategoriesLocal = new HashSet<string>(); // v141 (Localization check)
        HashSet<string> usedWords = new HashSet<string>();           // v140 (Word check)

        int currentSpriteCount = 0; // v135
        int textIndex = 0;          // v25
        int spriteIndex = 0;        // v134
        int currentTotalWords = 0;  // v136 (Loop count) -> v84

        // 主循环：直到分类数量达到 template.CategoryCnt (v28)
        while (generatedKvpDict.Count < template.CategoryCnt) {
            if (textIndex >= wordCandidates.Count) break; // 安全检查

            string rawCategoryName;
            bool isSprite;

            // 配额系统：优先满足图片卡
            if (currentSpriteCount < targetSpriteCount && spriteIndex < spriteCandidates.Count) {
                rawCategoryName = spriteCandidates[spriteIndex];
                spriteIndex++;
                isSprite = true;
            } else {
                rawCategoryName = wordCandidates[textIndex];
                textIndex++;
                isSprite = false;
            }

            // 查重逻辑 (基于本地化名称)
            string catKey = rawCategoryName.Split('#')[0];
            //string localizedCatName = UIUtils.Lang(GameplayUtils.GetCardLangInfoKey(catKey));
            string localizedCatName = GameplayUtils.GetCardLangInfoKey(catKey);

            if (usedCategoriesLocal.Contains(localizedCatName)) continue;

            // 创建对象
            WordKVPInfo newInfo = new WordKVPInfo();
            newInfo.Category = rawCategoryName;
            newInfo.UseSprite = isSprite;
            newInfo.Words = new List<string>();

            // 初始填充：确保每个分类至少有1个词
            List<string> availableWords = this.mCategoryDic[rawCategoryName];
            bool success = false;

            foreach (string word in Utils.GetRandomList(availableWords, availableWords.Count, randomSeed)) {
                if (!usedWords.Contains(word)) {
                    newInfo.Words.Add(word);
                    usedWords.Add(word);
                    // usedWords.Add(UIUtils.Lang(word));
                    success = true;
                    currentTotalWords++;
                    break;
                }
            }

            if (success) {
                // 存入字典
                generatedKvpDict.Add(rawCategoryName, newInfo);
                usedCategoriesLocal.Add(localizedCatName);
                if (isSprite) currentSpriteCount++;
            }
        }

        // -------------------------------------------------------------------------
        // 4. 补充单词 (Fill Words) - 修正后的逻辑
        // -------------------------------------------------------------------------

        // v99 = generatedKvpDict.Keys.ToList()
        List<string> chosenCategories = new List<string>(generatedKvpDict.Keys);

        // v100 = Shuffle(chosenCategories) using textCandidates.Count as size hint
        List<string> shuffledCategories = Utils.GetRandomList(chosenCategories, wordCandidates.Count, randomSeed);

        // 遍历打乱后的分类，尝试补词
        for (int i = 0; i < shuffledCategories.Count; i++) {
            if (currentTotalWords >= targetTotalWordCount) break;

            string catName = shuffledCategories[i];
            WordKVPInfo info = generatedKvpDict[catName];

            // 限制每个分类最多 8 个词 (v100 <= 7)
            if (info.Words.Count <= 7) {
                List<string> availableWords = this.mCategoryDic[catName];

                foreach (string word in Utils.GetRandomList(availableWords, availableWords.Count, randomSeed)) {
                    if (!usedWords.Contains(word)) {
                        info.Words.Add(word);
                        usedWords.Add(word);
                        // usedWords.Add(UIUtils.Lang(word));

                        currentTotalWords++;
                        break; // 每个分类这轮只加 1 个
                    }
                }
            }
        }

        // -------------------------------------------------------------------------
        // 5. 封装结果
        // -------------------------------------------------------------------------
        WordSotaLevelData finalLevel = new WordSotaLevelData();
        finalLevel.LevelID = levelId;
        finalLevel.Categories = new List<WordKVPInfo>(generatedKvpDict.Values); // Dictionary 转 List
        finalLevel.OperateSlotSizes = template.OperateSlotSizes;
        finalLevel.TargetSlotCnt = template.TargetSlotCnt;
        finalLevel.LeftMoveCnt = -1;
        finalLevel.DifficultyType = template.DifficultyType;

        return finalLevel;
    }
    public LoopLevelFileHandler() {
    }
}
