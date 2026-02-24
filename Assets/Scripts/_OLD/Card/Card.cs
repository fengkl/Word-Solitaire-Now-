using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class Card : MonoBehaviour
{
    [Header("卡牌信息")] 
    public GameObject parent; // 父物体
    public CardType cardType; // 卡牌类型
    public string cardCategory; // 卡牌分类
    public string cardName; // 卡牌名称

    [Header("卡牌图片和文字")] 
    public GameObject cardBack; // 卡背
    public GameObject cardFront; // 卡面
    public SpriteRenderer cardBackSr; // 卡背图片
    public SpriteRenderer cardSurfaceSr; // 卡面图片
    public SpriteRenderer cardBorderSr; // 卡边图片
    public Canvas cardCanvas; // 卡牌画布
    public TextMeshProUGUI nameText; // 名称文本

    [Header("点击缩放参数")] 
    public float duration; // 持续时间
    public Coroutine changeScaleCoroutine; // 缩放协程

    [Header("卡面文字和移动相关")] 
    public float canvasScaleValue; // 缩放值
    public float textMoveToYValue; // 文字移动到Y轴的值
    public float cardMoveToYValue; // 卡牌移动到Y轴的值
    public float originalCanvasScaleValue; // 原始缩放值
    public float originalTextMoveToYValue; // 原始文本Y轴的值
    public float originalCardMoveToYValue; // 原始卡牌Y轴的值
    public float moveDuration; // 文字移动持续时间
    public bool isTextScale; // 是否缩放了文字

    [Header("翻牌相关")]
    public float flipDuration; // 翻牌持续时间
    public bool isFaceDown; // 是否盖着牌
    
    /// <summary>
    /// 初始化卡牌信息
    /// </summary>
    public void Init()
    {
        nameText.text = cardName.Split("#")[0];
        isFaceDown = true;
    }
    
    /// <summary>
    /// 初始化卡牌信息
    /// </summary>
    public void Init(CardType type, string category, string name)
    {
        
        cardType = type;
        cardCategory = category;
        cardName = name;
        nameText.text = cardName.Split("#")[0];
        isFaceDown = true;
    }

    #region 点击时缩放相关
    
    /// <summary>
    /// 执行缩放
    /// </summary>
    /// <param name="scale"></param>
    public void StartChangeScale(float scale)
    {
        // 如果有协程在进行，先停止上一个协程
        if (changeScaleCoroutine != null)
        {
            StopCoroutine(changeScaleCoroutine);
        }
        
        // 开启缩放协程，让卡牌大小改变
        changeScaleCoroutine = StartCoroutine(ChangeScale(scale));
    }

    /// <summary>
    /// 鼠标点击缩放
    /// </summary>
    /// <param name="scale"></param>
    /// <returns></returns>
    public IEnumerator ChangeScale(float scale)
    {
        // 记录开始和结束
        Vector3 start = transform.localScale;
        Vector3 target = new Vector3(scale, scale, 1);
    
        float timer = 0;
    
        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(start, target, timer / duration);
            yield return null;
        }
        
        FlipCard();
    }
    
    #endregion
    
    #region 卡面文字和移动相关
    
    /// <summary>
    /// 移动牌面文字并变小
    /// </summary>
    public void MovingCardTextToSmall(int customDuration = -1)
    {
        if (isTextScale) return;
        
        isTextScale = true;

        // 隐藏边框
        cardBorderSr.enabled = false;
        
        // 创建序列动画
        Sequence sequence = DOTween.Sequence();

        // 缩放和移动文字
        sequence.Join(cardCanvas.transform.DOScale(canvasScaleValue, customDuration != -1 ? customDuration : moveDuration));
        sequence.Join(cardCanvas.transform.DOLocalMoveY(textMoveToYValue, customDuration != -1 ? customDuration : moveDuration));
    }
    
    /// <summary>
    /// 移动牌面文字并变大
    /// </summary>
    public void MovingCardTextToBig()
    {
        if (!isTextScale) return;
        
        isTextScale = false;

        // 显示边框
        cardBorderSr.enabled = true;
        
        // 创建序列动画
        Sequence sequence = DOTween.Sequence();

        // 缩放和移动文字
        sequence.Join(cardCanvas.transform.DOScale(originalCanvasScaleValue, moveDuration));
        sequence.Join(cardCanvas.transform.DOLocalMoveY(originalTextMoveToYValue, moveDuration));
    }

    /// <summary>
    /// 移动牌位置
    /// </summary>
    public void MovingCardPos(int customDuration = -1)
    {
        transform.DOLocalMoveY(cardMoveToYValue, customDuration != -1 ? customDuration : moveDuration);
    }
    
    #endregion

    #region 翻牌相关
    
    /// <summary>
    /// 翻转卡牌（翻至正面朝上）
    /// </summary>
    public void FlipCard(int customDuration = -1)
    {
        if (!isFaceDown) return;
        
        isFaceDown = false;
        
        // 创建序列动画
        Sequence sequence = DOTween.Sequence();
        
        sequence.Append(cardBackSr.transform.DOScaleX(0, customDuration != -1 ? customDuration : flipDuration));
        sequence.Append(cardFront.transform.DOScaleX(1, customDuration != -1 ? customDuration : flipDuration));
        sequence.AppendCallback(() =>
        {
            parent.GetComponent<CardGroup>().canSelected = true;
        });
    }
    
    /// <summary>
    /// 翻转卡牌（翻至背面朝上）
    /// </summary>
    public void ReverseFlipCard()
    {
        isFaceDown = true;
        
        // 创建序列动画
        Sequence sequence = DOTween.Sequence();
        
        sequence.Append(cardFront.transform.DOScaleX(0, flipDuration));
        sequence.Append(cardBackSr.transform.DOScaleX(1, flipDuration));
        sequence.AppendCallback(() =>
        {
            parent.GetComponent<CardGroup>().canSelected = false;
        });
    }
    
    #endregion
}
