using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MessagePack;
using System;

[MessagePackObject(false)]
[Serializable]
public class WordSlotInfo {
    [Key(0)]
    public List<string> Infos;

    public WordSlotInfo() {
    }
    // ÎªWordSlotInfo²¹³äToString
    public override string ToString() {
        if (Infos == null || Infos.Count == 0)
            return "  ÄÚÈÝ: ¿Õ";
        return $"  ÄÚÈÝ: [{string.Join(", ", Infos)}]";
    }
}
