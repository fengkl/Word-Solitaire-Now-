using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;
using System;
using System.Text;

[Serializable]
[MessagePackObject(false)]
public class WordSotaLevelData {
    [Key(0)]
    public int LevelID;

    [Key(1)]
    public List<WordKVPInfo> Categories;

    [Key(2)]
    public List<int> OperateSlotSizes;

    [Key(3)]
    public int TargetSlotCnt;

    [Key(4)]
    public int LeftMoveCnt;

    [Key(5)]
    public string DifficultyType;
    public WordSotaLevelData() {
    }
    /// <summary>
    /// 重写ToString，格式化输出所有字段内容
    /// </summary>
    /// <returns>结构化的字符串</returns>
    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        // 标题分隔线
        sb.AppendLine("=== WordSotaLevelData 基础关卡配置 ===");

        // 基础数值字段
        sb.AppendLine($"关卡ID: {LevelID}");
        sb.AppendLine($"目标槽数量: {TargetSlotCnt}");
        sb.AppendLine($"剩余移动次数: {LeftMoveCnt}");
        sb.AppendLine($"难度类型: {DifficultyType ?? "未设置"}");

        // 操作槽尺寸列表
        sb.AppendLine("\n【操作槽尺寸列表(OperateSlotSizes)】");
        if (OperateSlotSizes == null || OperateSlotSizes.Count == 0)
            sb.AppendLine("  无操作槽尺寸数据");
        else {
            sb.AppendLine($"  尺寸列表: [{string.Join(", ", OperateSlotSizes)}]");
            sb.AppendLine($"  操作槽总数: {OperateSlotSizes.Count}");
        }

        // 单词分类列表（核心嵌套字段）
        sb.AppendLine("\n【单词分类列表(Categories)】");
        if (Categories == null || Categories.Count == 0)
            sb.AppendLine("  无单词分类数据");
        else {
            for (int i = 0; i < Categories.Count; i++) {
                sb.AppendLine($"  分类{i + 1}:");
                // 复用WordKVPInfo的ToString，保证格式统一
                sb.Append(Categories[i]?.ToString() ?? "  无效分类数据");
            }
        }

        // 结尾分隔线
        sb.AppendLine("======================================");

        return sb.ToString();
    }
}