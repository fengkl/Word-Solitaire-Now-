using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;
using System;
using System.Text;

[Serializable]
[MessagePackObject(false)]
public class WordSotaFixedLevelData {

    [Key(0)]
    public int LevelID;

    [Key(1)]
    public LevelMode Mode;

    [Key(3)]
    public List<WordSlotInfo> OperateSlots;

    [Key(5)]
    public WordSlotInfo HomeSlot;

    [Key(4)]
    public List<WordSlotInfo> TargetSlots;


    public WordSotaFixedLevelData() {
    }


    /// <summary>
    /// 重写ToString，结构化输出所有固定关卡配置字段
    /// </summary>
    /// <returns>格式化的字符串</returns>
    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        // 标题分隔线，明确类名
        sb.AppendLine("=== WordSotaFixedLevelData 固定关卡配置 ===");

        // 基础字段（数值/枚举）
        sb.AppendLine($"固定关卡ID: {LevelID}");
        sb.AppendLine($"关卡模式: {Mode} (枚举值: {(int)Mode})");

        // 操作槽列表（List<WordSlotInfo>）
        sb.AppendLine("\n【固定操作槽列表(OperateSlots)】");
        if (OperateSlots == null || OperateSlots.Count == 0)
            sb.AppendLine("  无固定操作槽数据");
        else {
            for (int i = 0; i < OperateSlots.Count; i++) {
                sb.AppendLine($"  操作槽{i + 1}:");
                sb.AppendLine(OperateSlots[i]?.ToString() ?? "  无效操作槽数据");
            }
        }

        // 目标槽列表（List<WordSlotInfo>）
        sb.AppendLine("\n【固定目标槽列表(TargetSlots)】");
        if (TargetSlots == null || TargetSlots.Count == 0)
            sb.AppendLine("  无固定目标槽数据");
        else {
            for (int i = 0; i < TargetSlots.Count; i++) {
                sb.AppendLine($"  目标槽{i + 1}:");
                sb.AppendLine(TargetSlots[i]?.ToString() ?? "  无效目标槽数据");
            }
        }

        // 主场槽（单个WordSlotInfo）
        sb.AppendLine("\n【固定主场槽(HomeSlot)】");
        sb.AppendLine(HomeSlot?.ToString() ?? "  无固定主场槽数据");

        // 结尾分隔线，和标题呼应
        sb.AppendLine("==========================================");

        return sb.ToString();
    }
}
