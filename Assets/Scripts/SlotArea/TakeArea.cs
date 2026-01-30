using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class TakeArea : MonoBehaviour
{
    [Header("基本信息")]
    public Stack<CardGroup> cardGroups = new Stack<CardGroup>(); // 卡牌栈
    public float gapSize; // 卡牌间隔
    public float maxDisplayCount; // 最大显示数量
    public int originalSortingOrder; // 原始排序顺序
    public TakeAreaSlot leftTakeArea; // 左侧取牌区域
    public TakeAreaSlot rightTakeArea; // 右侧取牌区域
    
    [Header("初始化动画参数")]
    public float initMoveTime; // 移动时间

    [Header("翻牌动画参数")] 
    public float flipCD; // 翻牌冷却时间
    public float recycleCD; // 回收冷却时间
    public bool isMovingCard; // 是否正在移动牌
    public bool isRefreshing; // 是否正在刷新
    
    [Header("剩余数量显示")]
    public GameObject faceDownCount; // 剩余数量显示框
    public TextMeshProUGUI countText; // 剩余数量文本
    
    public GameObject slotPrefab; //卡槽预制体
    
    [Header("测试用")]
    public List<CardGroup> testCardGroups = new List<CardGroup>();

    private void Update()
    {
        testCardGroups = new List<CardGroup>(cardGroups);
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        GameObject slot = Instantiate(slotPrefab, transform.position, Quaternion.identity);
        slot.transform.parent = transform;
    }

    private void OnMouseDown()
    {
        if (GameManager.Instance.isInitializing || isMovingCard || isRefreshing) return;

        if (cardGroups.Count > 0)
        {
            isMovingCard = true;
            
            FlipCardAndMovingToTakeArea();
            
            Invoke("ChangeIsMovingCard", flipCD);
        }
        else
        {
            Stack<CardGroup> leftGroup = leftTakeArea.allCardGroups;
            Stack<CardGroup> rightGroup = rightTakeArea.allCardGroups;

            // 如果取牌区没牌了，则返回
            if (leftGroup.Count + rightGroup.Count == 0) return;
            
            isRefreshing = true;

            Sequence sequence;
            
            // 聚牌
            GatherCards(leftTakeArea);
            sequence = GatherCards(rightTakeArea);

            sequence.OnComplete(() =>
            {
                // 先将多的部分取出来
                if (leftGroup.Count > rightGroup.Count)
                {
                    AddCard(leftGroup.Pop());
                    ChangeFirstCardSortingOrder(cardGroups.Peek().cards[0]);
                }
            
                // 合并卡组
                while (leftGroup.Count > 0 || rightGroup.Count > 0)
                {
                    if (leftGroup.Count > 0)
                    {
                        AddCard(leftGroup.Pop());
                        ChangeFirstCardSortingOrder(cardGroups.Peek().cards[0]);
                    }

                    if (rightGroup.Count > 0)
                    {
                        AddCard(rightGroup.Pop());
                        ChangeFirstCardSortingOrder(cardGroups.Peek().cards[0]);
                    }
                }
                
                // 回收
                RecycleCards();
            });
            
            Invoke("ChangeIsRefreshing", recycleCD);
        }
    }

    /// <summary>
    /// 更新牌位置
    /// </summary>
    public void UpdateCardsPos()
    {
        int count = 0;
        
        // 设置每张卡牌的位置
        foreach (CardGroup cardGroup in cardGroups)
        {
            Vector3 target = new Vector3(transform.position.x - (count * gapSize), transform.position.y, count);

            cardGroup.transform.DOMove(target, initMoveTime)
                .SetEase(Ease.InOutQuad);
            
            // 限制显示的最大数量
            if (count < maxDisplayCount - 1)
            {
                count++;
            }
        }
        
        // 更新剩余数量
        UpdateCount();
    }

    /// <summary>
    /// 添加卡牌
    /// </summary>
    /// <param name="cardGroup"></param>
    public void AddCard(CardGroup cardGroup)
    {
        // 调整新增卡牌的图层先后
        if (cardGroups.Count > 0)
        {
            cardGroup.cards[0].cardSurfaceSr.sortingOrder = cardGroups.Peek().cards[0].cardSurfaceSr.sortingOrder + 5;
            if (cardGroup.cards[0].cardBorderSr != null)
            {
                cardGroup.cards[0].cardBorderSr.sortingOrder = cardGroups.Peek().cards[0].cardBorderSr.sortingOrder + 5;
            }
            cardGroup.cards[0].cardCanvas.sortingOrder = cardGroups.Peek().cards[0].cardCanvas.sortingOrder + 5;
            cardGroup.cards[0].cardBackSr.sortingOrder = cardGroups.Peek().cards[0].cardBackSr.sortingOrder + 5;
        }
        else
        {
            DisplayCount(cardGroup);
        }

        // 入栈
        cardGroups.Push(cardGroup);
    }

    /// <summary>
    /// 翻牌并移动到取牌区域
    /// </summary>
    private void FlipCardAndMovingToTakeArea()
    {
        CardGroup cardGroup1;
        CardGroup cardGroup2;

        if (cardGroups.Count > 1)
        {
            // 先后取两次
            cardGroup1 = cardGroups.Pop();
            cardGroup2 = cardGroups.Pop();
            
            // 分别向右和左添加牌组
            rightTakeArea.AddCard(cardGroup1);
            leftTakeArea.AddCard(cardGroup2);
        }
        else
        {
            // 只取一次，且添加到左侧
            cardGroup1 = cardGroups.Pop();
            leftTakeArea.AddCard(cardGroup1);
        }
        
        UpdateCardsPos();
    }

    /// <summary>
    /// 聚牌
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    private Sequence GatherCards(TakeAreaSlot slot)
    {
        // 创建序列动画
        Sequence sequence = DOTween.Sequence();
        
        // 移动牌组,将所有牌摞到一起
        foreach (var cardGroup in slot.allCardGroups)
        {
            sequence.Join(cardGroup.transform.DOMove(slot.transform.position, slot.moveToTakeAreaTime));
            cardGroup.cards[0].MovingCardTextToBig();
        }

        return sequence;
    }

    /// <summary>
    /// 回收牌组
    /// </summary>
    private void RecycleCards()
    {
        // 创建序列动画
        Sequence sequence = DOTween.Sequence();

        // 移动牌组,将所有牌放回去
        foreach (var cardGroup in cardGroups)
        {
            sequence.Join(cardGroup.transform.DOMove(transform.position, initMoveTime));
            cardGroup.cards[0].ReverseFlipCard();
        }

        sequence.AppendCallback(UpdateCardsPos);
    }

    /// <summary>
    /// 翻牌冷却结束
    /// </summary>
    private void ChangeIsMovingCard()
    {
        isMovingCard = false;
    }

    /// <summary>
    /// 回收冷却结束
    /// </summary>
    private void ChangeIsRefreshing()
    {
        isRefreshing = false;
    }

    /// <summary>
    /// 重置第一张牌的图层
    /// </summary>
    /// <param name="card"></param>
    private void ChangeFirstCardSortingOrder(Card card)
    {
        if (cardGroups.Count == 1)
        {
            card.cardSurfaceSr.sortingOrder = originalSortingOrder + 1;
            if (card.cardBorderSr != null)
            {
                card.cardBorderSr.sortingOrder = originalSortingOrder + 2;
            }
            card.cardCanvas.sortingOrder = originalSortingOrder + 3;
            card.cardBackSr.sortingOrder = originalSortingOrder;
        }
    }

    /// <summary>
    ///  显示数量
    /// </summary>
    public void DisplayCount(CardGroup cardGroup)
    {
        faceDownCount.SetActive(true);
        faceDownCount.transform.SetParent(cardGroup.transform);
    }

    /// <summary>
    /// 更新剩余数量
    /// </summary>
    public void UpdateCount()
    {
        if (cardGroups.Count > 0)
        {
            countText.text = cardGroups.Count.ToString();
        }
        else
        {
            faceDownCount.SetActive(false);
        }
    }
}
