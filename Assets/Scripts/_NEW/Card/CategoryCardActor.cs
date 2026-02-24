using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CategoryCardActor : CardActor
{
    [Header("分类卡独有")] 
    public Image textBg; // 文本背景
    public Text countText; // 计数文本
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
        nameText.text = cardName.Split("#")[0];
        isFaceDown = true;
        countText.text = $"0/{cardCount}";
        originScale = transform.localScale.x;
    }
}
