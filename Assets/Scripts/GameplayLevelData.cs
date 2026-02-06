using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GameplayLevelData {

    public int ID;
    public LevelMode Mode;
    public List<WordKVPInfo> KVPs;

    public List<WordSlotInfo> OperateSlots;

    public WordSlotInfo HomeSlot;

    public List<WordSlotInfo> TargetSlots;

    public int TargetSlotCnt;

    public int OperateSlotCnt;

    public int LeftMoveCnt;

    public int Seed;

    public string DifficultyType;

    private static StringBuilder mStrBuilder;

    public string ToBase64() {
        return null;
    }

    public static GameplayLevelData FromBase64(string utf8) {
        return null;
    }
    /// <summary>
    /// 重写ToString，清晰输出所有字段内容（处理空值，格式化显示）
    /// </summary>
    /// <returns>格式化的字符串</returns>
    public override string ToString() {
        // 使用StringBuilder提升拼接效率，格式化输出
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== GameplayLevelData 详情 ===");

        // 基础字段
        sb.AppendLine($"关卡ID: {ID}");
        sb.AppendLine($"关卡模式: {Mode} (值: {(int)Mode})");
        sb.AppendLine($"难度类型: {DifficultyType ?? "无"}");
        sb.AppendLine($"目标槽数量: {TargetSlotCnt}");
        sb.AppendLine($"操作槽数量: {OperateSlotCnt}");
        sb.AppendLine($"剩余移动次数: {LeftMoveCnt}");
        sb.AppendLine($"随机种子: {Seed}");

        // KVPs（单词分类列表）
        sb.AppendLine("\n【单词分类列表(KVPs)】");
        if (KVPs == null || KVPs.Count == 0)
            sb.AppendLine("  无分类数据");
        else {
            for (int i = 0; i < KVPs.Count; i++) {
                sb.AppendLine($"  分类{i + 1}:");
                sb.Append(KVPs[i]?.ToString() ?? "  无效分类数据");
            }
        }

        // 操作槽列表
        sb.AppendLine("\n【操作槽列表(OperateSlots)】");
        if (OperateSlots == null || OperateSlots.Count == 0)
            sb.AppendLine("  无操作槽数据");
        else {
            for (int i = 0; i < OperateSlots.Count; i++) {
                sb.AppendLine($"  操作槽{i + 1}:");
                sb.AppendLine(OperateSlots[i]?.ToString() ?? "  无效操作槽数据");
            }
        }

        // 目标槽列表
        sb.AppendLine("\n【目标槽列表(TargetSlots)】");
        if (TargetSlots == null || TargetSlots.Count == 0)
            sb.AppendLine("  无目标槽数据");
        else {
            for (int i = 0; i < TargetSlots.Count; i++) {
                sb.AppendLine($"  目标槽{i + 1}:");
                sb.AppendLine(TargetSlots[i]?.ToString() ?? "  无效目标槽数据");
            }
        }

        // 主场槽
        sb.AppendLine("\n【主场槽(HomeSlot)】");
        sb.AppendLine(HomeSlot?.ToString() ?? "  无主场槽数据");

        // 结尾分隔线
        sb.AppendLine("==============================");

        return sb.ToString();
    }

    public GameplayLevelData() {
    }
}
