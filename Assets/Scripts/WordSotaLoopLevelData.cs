using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MessagePack;


[System.Serializable]
[MessagePackObject(false)]
public class WordSotaLoopLevelData {

    [Key(0)]
    public int LoopID;

    [Key(1)]
    public int CategoryCnt;

    [Key(2)]
    public int MinWordCnt;
    [Key(3)]
    public int MaxWordCnt;

    [Key(4)]
    public int MinUseSpriteCnt;

    [Key(5)]
    public int MaxUseSpriteCnt;

    [Key(6)]
    public List<int> OperateSlotSizes;

    [Key(7)]
    public int TargetSlotCnt;

    [Key(8)]
    public string DifficultyType;

    public WordSotaLoopLevelData() {
    }
}
