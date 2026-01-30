using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class CategoryCard : Card
{
    [Header("计数")]
    public TextMeshProUGUI countText; // 计数文本
    public int cardCount; // 该种类下的卡牌数量

    /// <summary>
    /// 初始化卡牌信息
    /// </summary>
    public void Init(CardType type, string category, string name, int count)
    {
        cardType = type;
        cardCategory = category;
        cardName = name;
        cardCount = count;
        nameText.text = cardName;
        isFaceDown = true;
        countText.text = $"0/{cardCount}";
    }
}
