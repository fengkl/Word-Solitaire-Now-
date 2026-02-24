using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TargetSlot : Slot_New
{
    public CategoryCardActor categoryCard; // 当前卡槽的所装的分类卡
    public float moveDuration; // 移动到卡槽的时间
    
    [Header("打包动画参数")] 
    public float packDuration; // 打包移动的动画时间
    public float resetDuration; // 重置动画时间
    
    [Header("进度点")]
    public GameObject progressPointsBar; // 进度点显示条
    public List<Image> progressPointsList; // 进度点的图片
    
    /// <summary>
    /// 更新卡牌位置
    /// </summary>
    public override Sequence UpdateCardsPos()
    {
        // 创建动画序列
        Sequence sequence = DOTween.Sequence();
        
        // 如果卡槽仅有一张卡，那么一定是分类卡
        if (cards.Count == 1)
        {
            categoryCard.isMoving = true;
            
            // 移动卡牌位置
            categoryCard.transform.DOMove(transform.position, moveDuration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    categoryCard.isMoving = false;
                    categoryCard.canSelected = false;
                });
        }
        else
        {
            foreach (var card in cards)
            {
                if (card.cardType == CardType.Category && card.isTextScale) continue;
                
                card.isMoving = true;
                
                // 创建tween，为每一张牌单独设置OnComplete
                Tweener tween = card.transform.DOMove(transform.position, moveDuration);
                tween.SetEase(Ease.InOutQuad);
                tween.OnComplete(() => 
                {
                    card.isMoving = false;
                    card.canSelected = false;
                    card.transform.SetAsLastSibling();
                });
                
                sequence.Join(tween);
                
                card.MovingCardTextToBig();
            }
            
            sequence.AppendCallback(() =>
            {
                CreateProgressPoints(); // 创建进度点
                categoryCard.countText.enabled = false;
                categoryCard.MovingCardPos();
                categoryCard.MovingCardTextToSmall();

                // 处理牌组完成时的打包
                if (cards.Count == categoryCard.cardCount + 1)
                {
                    RemoveProgressPoints();
                    PackCards();
                }
            });
            
            LightUpProgressPoint(); // 点亮进度点
        }
        
        return sequence;
    }

    /// <summary>
    /// 更新keyPos和slotHeight
    /// </summary>
    public override void UpdateKeyPosAndHeight()
    {
        float posOffset = 0;
        float height = (transform as RectTransform).rect.height;

        if (cards.Count > 1)
        {
            // 计算keyPos偏移量，遇到盖牌向下移动满gapSize，遇到明牌则移动半个gapSize
            posOffset += categoryCard.cardMoveDistance / 2;
            // 计算高度，遇到盖牌不变，遇到明牌则增加gapSize的数值
            height += categoryCard.cardMoveDistance;
        }        
        
        keyPos = new Vector3(transform.position.x, transform.position.y + posOffset, 0);
        slotHeight = height;
    }
    
    #region 进度点相关

    /// <summary>
    /// 创建进度点
    /// </summary>
    public void CreateProgressPoints()
    {
        for (int i = 0; i < categoryCard.cardCount; i++)
        {
            progressPointsList[i].gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// 点亮进度点
    /// </summary>
    public void LightUpProgressPoint()
    {
        for (int i = 0; i < cards.Count - 1; i++)
        {
            progressPointsList[i].color = new Color(45/255f, 200/255f, 0);
        }
    }
    
    /// <summary>
    /// 移除进度点
    /// </summary>
    public void RemoveProgressPoints()
    {
        foreach (var progressPoint in progressPointsList)
        {
            progressPoint.color = new Color(100/255f, 50/255f, 15/255f);
            progressPoint.gameObject.SetActive(false);
        }
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
        
        sequence.Join(categoryCard.transform.DOMoveY(transform.position.y + (transform as RectTransform).rect.height * (transform as RectTransform).transform.localScale.y, packDuration));
        
        // 调整图层
        sequence.AppendCallback(() =>
        {
            categoryCard.transform.SetAsLastSibling();
        });
        
        sequence.Join(categoryCard.transform.DOMove(transform.position, packDuration));
    
        sequence.AppendCallback(() =>
        {
            // 将卡牌从卡槽中移除
            foreach (CardActor card in cards)
            {
                if (card.cardType == CardType.Character)
                {
                    card.gameObject.SetActive(false);
                }
            }
            
            // 完成计数+1
            GameDataUtils.Instance.completeCount++;
            GameDataUtils.Instance.solitaireScene.topGroup.gameProgressBar.SetProgress(GameDataUtils.Instance.completeCount, GameDataUtils.Instance.aimCount);
    
            // 重置卡槽
            if (GameDataUtils.Instance.aimCount - GameDataUtils.Instance.completeCount >= 4)
            {
                ReSet();
            }
            
            // 判断是否完成游戏
            GameDataUtils.Instance.solitaireScene.CheckComplete();
        });
    }
    
    /// <summary>
    /// 重置
    /// </summary>
    public void ReSet()
    {
        // 创建动画序列
        Sequence sequence = DOTween.Sequence();
        
        // 缩小并消失
        sequence.Join(categoryCard.transform.DOScale(Vector3.zero, resetDuration));
        sequence.Join(categoryCard.cardFace.DOFade(0, resetDuration));
        sequence.Join(categoryCard.cardBorder.DOFade(0, resetDuration));
        sequence.Join(categoryCard.nameText.DOFade(0, resetDuration));
    
        // 重置卡槽
        sequence.AppendCallback(() =>
        {
            cards.Clear();
            categoryCard = null;
        });
    }
    
    # endregion
}
