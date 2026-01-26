using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StackAreaSlot : Slot
{
    public float maxCardCount; // 压缩前的最大存储容量
    public float maxHigh; // 压缩前的最大高度
    public float gapSize; // 两牌之间的间距
}
