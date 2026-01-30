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
    
    public List<Card> testCards = new List<Card>(); // 存储的卡牌
    public List<CardGroup> testGroups = new List<CardGroup>(); // 存储的牌组

    void Update()
    {
        testCards = allCards.ToList();
        testGroups = allCardGroups.ToList();
        
        CheckFlipCard();
    }
    
    /// <summary>
    /// 添加牌组
    /// </summary>
    /// <param name="cardGroup"></param>
    /// <param name="index"></param>
    public void AddCard(CardGroup cardGroup)
    {
        // 调整新增卡牌的图层先后
        if (allCardGroups.Count > 0)
        {
            cardGroup.cards[0].cardSurfaceSr.sortingOrder = allCardGroups.Peek().cards[0].cardSurfaceSr.sortingOrder + 5;
            if (cardGroup.cards[0].cardBorderSr != null)
            {
                cardGroup.cards[0].cardBorderSr.sortingOrder = allCardGroups.Peek().cards[0].cardBorderSr.sortingOrder + 5;
            }
            cardGroup.cards[0].cardCanvas.sortingOrder = allCardGroups.Peek().cards[0].cardCanvas.sortingOrder + 5;
            cardGroup.cards[0].cardBackSr.sortingOrder = allCardGroups.Peek().cards[0].cardBackSr.sortingOrder + 5;
        }

        // 设置当前卡槽
        cardGroup.currentSlot = this;
        
        // 入栈
        allCardGroups.Push(cardGroup);
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
