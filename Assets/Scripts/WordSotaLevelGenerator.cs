using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = System.Random;

// ===================== 修正后的生成器核心类 =====================
/// <summary>
/// 单词接龙关卡数据生成器（完全匹配真实WordSotaLevelData字段）
/// </summary>
public class WordSotaLevelGenerator {
    /// <summary>
    /// 生成关卡数据（✅ 完全匹配参数+字段，无编译错误）
    /// </summary>
    /// <param name="data">真实关卡配置数据（WordSotaLevelData）</param>
    /// <param name="randomSeed">随机种子（默认0）</param>
    /// <returns>可序列化的GameplayLevelData</returns>
    public static GameplayLevelData GenerateLevelData(WordSotaLevelData data, int randomSeed = 0) {
        // 1. 严格校验输入参数，避免空引用
        if (data == null)
            throw new ArgumentNullException(nameof(data), "WordSotaLevelData配置不能为空");
        if (data.Categories == null || data.Categories.Count == 0)
            throw new ArgumentException("Categories（单词分类列表）不能为空", nameof(data.Categories));
        if (data.OperateSlotSizes == null)
            data.OperateSlotSizes = new List<int>(); // 兜底空列表，避免后续报错

        // 2. 初始化随机数生成器
        Random random = new Random(randomSeed);

        // 3. 初始化输出的GameplayLevelData，映射核心字段
        GameplayLevelData levelData = new GameplayLevelData {
            ID = data.LevelID,
            Mode = LevelMode.Normal, // 补充默认模式（可根据需求扩展）
            KVPs = data.Categories,  // 核心映射：Categories → KVPs
            TargetSlotCnt = data.TargetSlotCnt,
            OperateSlotCnt = data.OperateSlotSizes.Count, // 操作槽数量=尺寸列表长度
            LeftMoveCnt = data.LeftMoveCnt,
            DifficultyType = data.DifficultyType ?? "Normal",
            Seed = randomSeed
        };

        // 4. 初始化去重集合：分类名（Category）、单词（Words）
        HashSet<string> categorySet = new HashSet<string>();
        HashSet<string> wordSet = new HashSet<string>();

        // 遍历Categories填充去重集合（核心数据源）
        foreach (WordKVPInfo kvp in data.Categories) {
            // 分类名去重
            if (!string.IsNullOrEmpty(kvp.Category))
                categorySet.Add(kvp.Category);

            // 单词列表去重
            if (kvp.Words != null && kvp.Words.Count > 0) {
                foreach (string word in kvp.Words) {
                    if (!string.IsNullOrEmpty(word))
                        wordSet.Add(word);
                }
            }
        }
        
        // 5. 随机打乱列表（保证种子可复现）
        List<string> mixedList = new List<string>();
        mixedList.AddRange(wordSet); // 加入所有单词
        mixedList.AddRange(categorySet); // 加入所有分类
        
        // 根据种子打乱混合列表
        List<string> randomMixedList = GetRandomList(mixedList, randomSeed);
        
        int totalIdx = 0;
        
        // 6. 生成目标槽（TargetSlots）
        levelData.TargetSlots = new List<WordSlotInfo>();
        int filledTargetSlots = 0;
        
        while (filledTargetSlots < data.TargetSlotCnt) {
            WordSlotInfo targetSlot = new WordSlotInfo();
            targetSlot.Infos = new List<string>();
        
            levelData.TargetSlots.Add(targetSlot);
            filledTargetSlots++;
        }
        
        // 7. 生成操作槽（OperateSlots）：匹配OperateSlotSizes尺寸
        levelData.OperateSlots = new List<WordSlotInfo>();
        for (int i = 0; i < data.OperateSlotSizes.Count; i++) {
            WordSlotInfo operateSlot = new WordSlotInfo();
            operateSlot.Infos = new List<string>();
        
            // 按OperateSlotSizes指定的尺寸填充单词/分类名
            int slotSize = data.OperateSlotSizes[i];
            for (int j = 0; j < slotSize; j++) {
                if (totalIdx < randomMixedList.Count) {
                    operateSlot.Infos.Add(randomMixedList[totalIdx]);
                    totalIdx++;
                } else {
                    break; // 无数据时停止填充
                }
            }
        
            levelData.OperateSlots.Add(operateSlot);
        }
        
        // 8. 填充主场槽（HomeSlot）：剩余未使用的单词/分类名
        levelData.HomeSlot = new WordSlotInfo();
        levelData.HomeSlot.Infos = new List<string>();
        
        // 填充剩余单词
        while (totalIdx < randomMixedList.Count) {
            levelData.HomeSlot.Infos.Add(randomMixedList[totalIdx]);
            totalIdx++;
        }
        
        
        // // 5. 随机打乱列表（保证种子可复现）
        // List<string> randomWordList = GetRandomList(wordSet.ToList(), randomSeed);
        // List<string> randomCategoryList = GetRandomList(categorySet.ToList(), randomSeed);
        //
        // // 6. 生成目标槽（TargetSlots）
        // levelData.TargetSlots = new List<WordSlotInfo>();
        // int filledTargetSlots = 0;
        // // 单槽最大单词数：最多3个（匹配反编译逻辑）
        // // int maxWordPerTargetSlot = categorySet.Count >= 3 ? 3 : categorySet.Count;
        // int maxWordPerTargetSlot = 0;
        //
        // // 索引变量（提升作用域，避免编译错误）
        // int wordIdx = 0;
        // int cateIdx = 0;
        //
        // while (filledTargetSlots < data.TargetSlotCnt) {
        //     WordSlotInfo targetSlot = new WordSlotInfo();
        //     targetSlot.Infos = new List<string>();
        //
        //     // 随机生成当前目标槽的单词数量（1 ~ maxWordPerTargetSlot）
        //     // int wordCount = random.Next(1, maxWordPerTargetSlot + 1);
        //     int wordCount = 0;
        //     
        //     for (int i = 0; i < wordCount; i++) {
        //         // 85%概率取单词，15%概率取分类名（反编译核心逻辑）
        //         double randomRate = random.NextDouble();
        //         if (randomRate < 0.85) {
        //             if (wordIdx < randomWordList.Count) {
        //                 targetSlot.Infos.Add(randomWordList[wordIdx]);
        //                 wordIdx++;
        //             } else {
        //                 targetSlot.Infos.Add(randomCategoryList[cateIdx]);
        //                 cateIdx++;
        //             }
        //         } else {
        //             if (cateIdx < randomCategoryList.Count) {
        //                 targetSlot.Infos.Add(randomCategoryList[cateIdx]);
        //                 cateIdx++;
        //             } else {
        //                 targetSlot.Infos.Add(randomWordList[wordIdx]);
        //                 wordIdx++;
        //             }
        //         }
        //     }
        //
        //     levelData.TargetSlots.Add(targetSlot);
        //     filledTargetSlots++;
        // }
        //
        // // 7. 生成操作槽（OperateSlots）：匹配OperateSlotSizes尺寸
        // levelData.OperateSlots = new List<WordSlotInfo>();
        // for (int i = 0; i < data.OperateSlotSizes.Count; i++) {
        //     WordSlotInfo operateSlot = new WordSlotInfo();
        //     operateSlot.Infos = new List<string>();
        //
        //     // 按OperateSlotSizes指定的尺寸填充单词/分类名
        //     int slotSize = data.OperateSlotSizes[i];
        //     for (int j = 0; j < slotSize; j++) {
        //         if (wordIdx < randomWordList.Count) {
        //             operateSlot.Infos.Add(randomWordList[wordIdx]);
        //             wordIdx++;
        //         } else if (cateIdx < randomCategoryList.Count) {
        //             operateSlot.Infos.Add(randomCategoryList[cateIdx]);
        //             cateIdx++;
        //         } else {
        //             break; // 无数据时停止填充
        //         }
        //     }
        //
        //     levelData.OperateSlots.Add(operateSlot);
        // }
        //
        // // 8. 填充主场槽（HomeSlot）：剩余未使用的单词/分类名
        // levelData.HomeSlot = new WordSlotInfo();
        // levelData.HomeSlot.Infos = new List<string>();
        //
        // // 填充剩余单词
        // while (wordIdx < randomWordList.Count) {
        //     levelData.HomeSlot.Infos.Add(randomWordList[wordIdx]);
        //     wordIdx++;
        // }
        //
        // // 填充剩余分类名
        // while (cateIdx < randomCategoryList.Count) {
        //     levelData.HomeSlot.Infos.Add(randomCategoryList[cateIdx]);
        //     cateIdx++;
        // }

        return levelData;
    }


    // 反编译中的固定标记字符串（需根据游戏真实业务替换）
    private const string StringLiteral_921 = "*";    // 空占位符（反编译中StringLiteral_921）
    private const string StringLiteral_462 = "#"; // 默认标记（反编译中StringLiteral_462）

    /// <summary>
    /// 生成关卡数据（带固定关卡配置）
    /// </summary>
    /// <param name="data">基础关卡配置（WordSotaLevelData）</param>
    /// <param name="fixedData">固定关卡配置（WordSotaFixedLevelData）</param>
    /// <param name="randomSeed">随机种子（默认0）</param>
    /// <returns>可序列化的GameplayLevelData</returns>
    public static GameplayLevelData GenerateLevelData(WordSotaLevelData data, WordSotaFixedLevelData fixedData, int randomSeed = 0) {
        // 1. 严格入参校验（避免空引用崩溃）
        if (data == null)
            throw new ArgumentNullException(nameof(data), "WordSotaLevelData配置不能为空");
        if (fixedData == null)
            throw new ArgumentNullException(nameof(fixedData), "WordSotaFixedLevelData配置不能为空");
        if (data.Categories == null)
            data.Categories = new List<WordKVPInfo>();
        // 兜底fixedData的槽数据，避免空引用
        if (fixedData.TargetSlots == null)
            fixedData.TargetSlots = new List<WordSlotInfo>();
        if (fixedData.OperateSlots == null)
            fixedData.OperateSlots = new List<WordSlotInfo>();
        if (fixedData.HomeSlot == null)
            fixedData.HomeSlot = new WordSlotInfo { Infos = new List<string>() };

        // 2. 初始化去重集合：分类名集合（对应反编译v3）、单词集合（对应反编译v4）
        HashSet<string> categorySet = new HashSet<string>();
        HashSet<string> wordSet = new HashSet<string>();

        // 3. 遍历data.Categories，填充基础去重集合（反编译核心循环）
        foreach (WordKVPInfo kvp in data.Categories) {
            // 填充分类名到集合（反编译中Add Category）
            if (!string.IsNullOrEmpty(kvp.Category))
                categorySet.Add(kvp.Category);

            // 填充单词到集合（反编译中Add Words）
            if (kvp.Words != null && kvp.Words.Count > 0) {
                foreach (string word in kvp.Words) {
                    if (!string.IsNullOrEmpty(word))
                        wordSet.Add(word);
                }
            }
        }

        // 4. 核心过滤：遍历fixedData的槽数据，移除非固定标记的元素（反编译核心逻辑）
        // 4.1 过滤固定目标槽（fixedData.TargetSlots → 反编译a2[6]）
        foreach (WordSlotInfo slot in fixedData.TargetSlots) {
            if (slot?.Infos == null) continue;
            foreach (string info in slot.Infos)
                RemoveNonFixedString(info, ref categorySet, ref wordSet);
        }

        // 4.2 过滤固定操作槽（fixedData.OperateSlots → 反编译a2[4]）
        foreach (WordSlotInfo slot in fixedData.OperateSlots) {
            if (slot?.Infos == null) continue;
            foreach (string info in slot.Infos)
                RemoveNonFixedString(info, ref categorySet, ref wordSet);
        }

        // 4.3 过滤固定主场槽（fixedData.HomeSlot → 反编译a2[5]）
        if (fixedData.HomeSlot?.Infos != null) {
            foreach (string info in fixedData.HomeSlot.Infos)
                RemoveNonFixedString(info, ref categorySet, ref wordSet);
        }

        // 5. 随机打乱过滤后的列表（对应反编译Utils__GetRandomList）
        List<string> randomWordList = GetRandomList(wordSet.ToList(), randomSeed);
        List<string> randomCategoryList = GetRandomList(categorySet.ToList(), randomSeed);

        // 6. 初始化GameplayLevelData并填充基础字段
        GameplayLevelData levelData = new GameplayLevelData {
            ID = data.LevelID,
            Mode = fixedData.Mode, // 复用固定关卡的模式
            KVPs = data.Categories,
            TargetSlotCnt = data.TargetSlotCnt,
            OperateSlotCnt = data.OperateSlotSizes?.Count ?? 0,
            LeftMoveCnt = data.LeftMoveCnt,
            DifficultyType = data.DifficultyType ?? "Normal",
            Seed = randomSeed,
            // 初始化槽列表
            TargetSlots = new List<WordSlotInfo>(),
            OperateSlots = new List<WordSlotInfo>(),
            HomeSlot = new WordSlotInfo { Infos = new List<string>() }
        };

        // 7. 填充目标槽（复用fixedData.TargetSlots + 随机补充）
        foreach (WordSlotInfo fixedSlot in fixedData.TargetSlots) {
            WordSlotInfo newSlot = new WordSlotInfo { Infos = new List<string>() };
            if (fixedSlot?.Infos == null) {
                levelData.TargetSlots.Add(newSlot);
                continue;
            }
            
            // 按固定标记填充：标记→随机数据，非标记→固定值
            foreach (string info in fixedSlot.Infos) {
                if (info == StringLiteral_921) {
                    // 填充随机单词（对应反编译中randomWordList）
                    if (randomWordList.Count > 0) {
                        newSlot.Infos.Add(randomWordList[0]);
                        randomWordList.RemoveAt(0);
                    }
                } else if (info == StringLiteral_462) {
                    // 填充随机分类名（对应反编译中randomCategoryList）
                    if (randomCategoryList.Count > 0) {
                        newSlot.Infos.Add(randomCategoryList[0]);
                        randomCategoryList.RemoveAt(0);
                    }
                } else {
                    // 填充固定值
                    newSlot.Infos.Add(info);
                }
            }
            levelData.TargetSlots.Add(newSlot);
        }

        // 8. 填充操作槽（直接复用fixedData.OperateSlots）
        foreach (WordSlotInfo fixedSlot in fixedData.OperateSlots) {
            // WordSlotInfo newSlot = new WordSlotInfo {
            //     Infos = fixedSlot?.Infos != null ? new List<string>(fixedSlot.Infos) : new List<string>()
            // };
            // levelData.OperateSlots.Add(newSlot);
            
            WordSlotInfo newSlot = new WordSlotInfo { Infos = new List<string>() };
            if (fixedSlot?.Infos == null) {
                levelData.OperateSlots.Add(newSlot);
                continue;
            }
    
            // 按固定标记填充：标记→随机数据，非标记→固定值（和TargetSlots相同的逻辑）
            foreach (string info in fixedSlot.Infos) {
                if (info == StringLiteral_921) {  // "*"
                    // 填充随机单词
                    if (randomWordList.Count > 0) {
                        newSlot.Infos.Add(randomWordList[0]);
                        randomWordList.RemoveAt(0);
                    }
                } else if (info == StringLiteral_462) {  // "#"
                    // 填充随机分类名
                    if (randomCategoryList.Count > 0) {
                        newSlot.Infos.Add(randomCategoryList[0]);
                        randomCategoryList.RemoveAt(0);
                    }
                } else {
                    // 填充固定值
                    newSlot.Infos.Add(info);
                }
            }
            levelData.OperateSlots.Add(newSlot);
        }

        // 9. 填充主场槽（复用fixedData.HomeSlot + 随机补充）
        if (fixedData.HomeSlot?.Infos != null) {
            foreach (string info in fixedData.HomeSlot.Infos) {
                if (info == StringLiteral_921) {
                    if (randomWordList.Count > 0) {
                        levelData.HomeSlot.Infos.Add(randomWordList[0]);
                        randomWordList.RemoveAt(0);
                    }
                } else if (info == StringLiteral_462) {
                    if (randomCategoryList.Count > 0) {
                        levelData.HomeSlot.Infos.Add(randomCategoryList[0]);
                        randomCategoryList.RemoveAt(0);
                    }
                } else
                    levelData.HomeSlot.Infos.Add(info);
            }
        }

        return levelData;
    }

    /// <summary>
    /// 辅助方法：移除非固定标记的字符串（反编译核心过滤逻辑）
    /// </summary>
    /// <param name="info">待检查的字符串</param>
    /// <param name="categorySet">分类名集合</param>
    /// <param name="wordSet">单词集合</param>
    private static void RemoveNonFixedString(string info, ref HashSet<string> categorySet, ref HashSet<string> wordSet) {
        if (string.IsNullOrEmpty(info)) return;

        // 反编译逻辑：如果不是固定标记字符串，从两个集合中移除
        if (info != StringLiteral_921 && info != StringLiteral_462) {
            wordSet.Remove(info);       // 从单词集合移除
            categorySet.Remove(info);   // 从分类名集合移除
        }
    }
    /// <summary>
    /// 静态工具方法：按随机种子打乱列表（保证结果可复现）
    /// </summary>
    private static List<T> GetRandomList<T>(List<T> source, int seed) {
        if (source == null || source.Count == 0)
            return new List<T>();

        Random random = new Random(seed);
        return source.OrderBy(item => random.Next()).ToList();
    }

}