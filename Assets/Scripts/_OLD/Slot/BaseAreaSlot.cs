using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BaseAreaSlot : Slot
{
    // 初始数据
    public List<string> cardInfos;
    
    public CategoryCard categoryCard; // 卡槽目前所装的卡牌
    public string categoryData; // 卡槽目前所装的分类
    public int cardCount; // 卡牌数量
    public GameObject progressPoints; // 进度点显示条
    public GameObject progressPointPrefab; // 进度点
    public List<Image> progressPointSrs; // 进度点的图片

    [Header("打包动画参数")] 
    public float packUpDuration; // 向上移动的动画时间
    public float packDownDuration; // 向下移动的动画时间
    public float packUpDistance; // 向上移动的距离
    public float packDownDistance; // 向下移动的距离
    
    [Header("重置参数")]
    public float resetDuration; // 重置动画时间
    
    public List<Card> testCards = new List<Card>(); // 存储的卡牌
    public List<CardGroup> testGroups = new List<CardGroup>(); // 存储的牌组

    void Update()
    {
        testCards = allCards.ToList();
        testGroups = allCardGroups.ToList();
    }

    /// <summary>
    /// 设置分类卡
    /// </summary>
    public void SetCategoryCard(CategoryCard card)
    {
        categoryCard = card;
        cardCount = card.cardCount;
    }
    
    #region 进度点相关

    /// <summary>
    /// 创建进度点
    /// </summary>
    public void CreateProgressPoints()
    {
        for (int i = 0; i < cardCount; i++)
        {
            GameObject point = Instantiate(progressPointPrefab, progressPoints.transform);
            point.transform.SetParent(progressPoints.transform, false);
            progressPointSrs.Add(point.GetComponent<Image>());
        }
    }

    /// <summary>
    /// 点亮进度点
    /// </summary>
    public void LightUpProgressPoint()
    {
        for (int i = 0; i < allCards.Count - 1; i++)
        {
            progressPointSrs[i].color = new Color(45/255f, 200/255f, 0);
        }
    }

    /// <summary>
    /// 移除进度点
    /// </summary>
    public void RemoveProgressPoints()
    {
        progressPoints.SetActive(false);
    }
    
    #endregion
    
    #region 卡组完成后的处理
    
    /// <summary>
    /// 将分类完成的卡牌全部打包
    /// </summary>
    public void PackCards()
    {
        // 创建动画序列
        Sequence sequence = DOTween.Sequence();
        
        // 让文本变大
        categoryCard.MovingCardTextToBig();
        
        sequence.Join(categoryCard.transform.DOMoveY(transform.position.y + packUpDistance, packUpDuration));
        
        // 调整图层
        sequence.AppendCallback(() =>
        {
            categoryCard.cardSurfaceSr.sortingOrder = allCardGroups.Peek().cards[0].cardSurfaceSr.sortingOrder + 5;
            if (categoryCard.cardBorderSr != null)
            {
                categoryCard.cardBorderSr.sortingOrder = allCardGroups.Peek().cards[0].cardBorderSr.sortingOrder + 5;
            }
            categoryCard.cardCanvas.sortingOrder = allCardGroups.Peek().cards[0].cardCanvas.sortingOrder + 5;
        });
        
        sequence.Join(categoryCard.transform.DOMove(transform.position, packDownDuration));

        sequence.AppendCallback(() =>
        {
            // 将卡牌从卡槽中移除
            foreach (Card card in allCards)
            {
                if (card.cardType == CardType.Character)
                {
                    Destroy(card.gameObject);
                }
            }
            
            // 完成计数+1
            GameManager.Instance.completeCount++;
            GameManager.Instance.SetProgress();

            // 重置卡槽
            if (GameManager.Instance.aimCount - GameManager.Instance.completeCount >= 4)
            {
                ReSet();
            }
        });
    }

    /// <summary>
    /// 重置
    /// </summary>
    public void ReSet()
    {
        CardGroup cardGroup = allCardGroups.Pop();
        
        // 创建动画序列
        Sequence sequence = DOTween.Sequence();
        
        // 缩小并消失
        sequence.Join(cardGroup.transform.DOScale(Vector3.zero, resetDuration));
        sequence.Join(categoryCard.cardSurfaceSr.DOFade(0, resetDuration));
        sequence.Join(categoryCard.cardBorderSr.DOFade(0, resetDuration));
        sequence.Join(categoryCard.nameText.DOFade(0, resetDuration));

        // 重置卡槽
        sequence.AppendCallback(() =>
        {
            isOccupied = false;
            allCardGroups.Clear();
            allCards.Clear();
            
            categoryCard = null;
            categoryData = null;
            cardCount = 0;
        });
    }
    
    # endregion
}
