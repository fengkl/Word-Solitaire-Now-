using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StackAreaSlot : Slot
{
    // 初始数据
    public List<string> cardInfos;
    
    public float maxCardCount; // 压缩前的最大存储容量
    public float maxHigh; // 压缩前的最大高度
    public float gapSize; // 两牌之间的间距
    
    public List<Card> testCards = new List<Card>(); // 存储的卡牌
    public List<CardGroup> testGroups = new List<CardGroup>(); // 存储的牌组
    

    void Update()
    {
        testCards = allCards.ToList();
        testGroups = allCardGroups.ToList();
        
        CheckFlipCard();
    }
    
    /// <summary>
    /// 检查是否有可翻转的卡牌
    /// </summary>
    private void CheckFlipCard()
    {
        if (GameManager.Instance.isMovingCard || GameManager.Instance.isInitializing) return;

        if (allCards.Count > 0 && allCards.Peek().isFaceDown)
        {
            allCards.Peek().FlipCard();
        }
    }
}
