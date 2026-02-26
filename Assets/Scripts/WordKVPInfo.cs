using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;
using System;
using System.Text;

[MessagePackObject(false)]
[Serializable]
public class WordKVPInfo {
    [Key(0)]
    public string Category;

    [Key(1)]
    public List<string> Words;

    [Key(2)]
    public bool UseSprite;
    public WordKVPInfo() {
    }

    // 为WordKVPInfo也补充ToString，方便嵌套显示
    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"  分类名: {Category ?? "无"}");
        sb.AppendLine($"  是否使用精灵: {UseSprite}");
        sb.Append($"  单词列表: ");
        if (Words == null || Words.Count == 0)
            sb.AppendLine("空");
        else
            sb.AppendLine($"[{string.Join(", ", Words)}]");
        return sb.ToString();
    }
}
