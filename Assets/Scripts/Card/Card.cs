using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public enum CardType
{
    Character, // 文字卡
    Category, // 分类卡
    Cover // 盖牌
}

public class Card : MonoBehaviour
{
    [Header("卡牌信息")] 
    public GameObject parent; // 父物体
    public CardType cardType; // 卡牌类型
    public CategoryData category; // 卡牌分类
    public string cardName; // 卡牌名称
    public SpriteRenderer cardSurfaceSr; // 卡面图片
    public SpriteRenderer cardBorderSr; // 卡边图片
    public Canvas cardCanvas; // 卡牌画布
    public TextMeshProUGUI nameText; // 名称文本

    [Header("拿起卡牌相关")]
    public Card nextCard; // 后一张卡牌]

    [Header("点击缩放参数")] 
    public float duration; // 持续时间
    public Coroutine changeScaleCoroutine; // 缩放协程

    [Header("卡面文字相关")] 
    public float canvasScaleValue; // 缩放值
    public float moveToYValue; // 移动到Y轴的值
    public float textMoveDuration; // 文字移动持续时间
    public bool isTextScale; // 是否缩放了文字
    
    /// <summary>
    /// 初始化卡牌信息
    /// </summary>
    public void Init()
    {
        nameText.text = cardName;
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
    }
    
    #endregion
    
    /// <summary>
    /// 移动牌面文字
    /// </summary>
    public void MovingCardText()
    {
        isTextScale = true;

        // 隐藏边框
        cardBorderSr.enabled = false;
        
        // 创建序列动画
        Sequence sequence = DOTween.Sequence();

        // 缩放和移动文字
        sequence.Join(cardCanvas.transform.DOScale(canvasScaleValue, textMoveDuration));
        sequence.Join(cardCanvas.transform.DOLocalMoveY(moveToYValue, textMoveDuration));
    }
    
    /// <summary>
    /// 寻找最顶部的牌
    /// </summary>
    /// <returns></returns>
    public Card FindNextCard()
    {
        return nextCard != null ? nextCard.FindNextCard() : this;
    }
}
