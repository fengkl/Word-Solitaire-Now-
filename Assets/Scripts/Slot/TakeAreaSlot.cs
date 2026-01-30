using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TakeAreaSlot : Slot
{
    [Header("卡牌信息")]
    public float frontGapSize; // 有字卡牌间隔
    public float backGapSize; // 折叠卡牌间隔
    public float maxFrontDisplayCount = 3; // 最大有字卡牌显示数量
    public float maxBackDisplayCount = 3; // 最大折叠卡牌显示数量

    [Header("动画参数")]
    public float moveToTakeAreaTime; // 移动时间
    
    
    [Header("测试用")]
    public List<CardGroup> testCardGroups = new List<CardGroup>();

    private void Update()
    {
        testCardGroups = new List<CardGroup>(allCardGroups);
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
        
        // 移动动画
        MoveCardGroupsToUp();
        cardGroup.cards[0].FlipCard();
    }

    /// <summary>
    /// 向上移动牌组
    /// </summary>
    public void MoveCardGroupsToUp()
    {
        int count = 0;

        foreach (var cardGroup in allCardGroups)
        {
            if (count == 0)
            {
                cardGroup.transform.DOMove(new Vector3(transform.position.x, transform.position.y, transform.position.z + 1), moveToTakeAreaTime);
                cardGroup.canSelected = true;
                cardGroup.cards[0].MovingCardTextToBig();
                count++;
            }
            else if (count < maxFrontDisplayCount)
            {
                cardGroup.transform.DOMove(new Vector3(transform.position.x, 
                                                       transform.position.y + frontGapSize * count, 
                                                       count + 1), 
                                            moveToTakeAreaTime);
                cardGroup.canSelected = false;
                cardGroup.cards[0].MovingCardTextToSmall();
                count++;
            }
            else if (count < maxFrontDisplayCount + maxBackDisplayCount)
            {
                cardGroup.transform.DOMove(new Vector3(transform.position.x, 
                                                       transform.position.y + frontGapSize * (maxFrontDisplayCount - 1) + backGapSize * (count - maxFrontDisplayCount + 1), 
                                                       count + 1), 
                                            moveToTakeAreaTime);
                count++;
            }
        }
    }
}
