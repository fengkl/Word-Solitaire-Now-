using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Slot : MonoBehaviour
{
    public SlotType slotType;
    public Stack<Card> allCards = new Stack<Card>(); // 所有卡牌
    public Stack<CardGroup> allCardGroups = new Stack<CardGroup>(); // 卡组
    public bool isOccupied; // 是否被占用


    [Header("边缘高亮相关参数")] 
    public float originHeight; // 原始高度
    public float borderHighLightInOutTime; // 边缘高亮进入退出时间
    public bool isLighting; // 是否高亮
    public SpriteRenderer borderRenderer; // 边缘高亮渲染器
    private Coroutine borderHighLightCoroutine; // 改变边缘高亮协程
    
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
    /// 改变边缘高亮协程
    /// </summary>
    /// <returns></returns>
    private IEnumerator borderHighLight(float alpha)
    {
        Color start = borderRenderer.color;
        Color target = new Color(borderRenderer.color.r, borderRenderer.color.g, borderRenderer.color.b, alpha);

        float timer = 0;

        while (timer < borderHighLightInOutTime)
        {
            timer += Time.deltaTime;
            borderRenderer.color = Color.Lerp(start, target, timer / borderHighLightInOutTime);
            yield return null;
        }
    }

    /// <summary>
    /// 启用边缘高亮
    /// </summary>
    public void EnableHighLight()
    {
        isLighting = true;
        
        if (borderHighLightCoroutine != null)
        {
            StopCoroutine(borderHighLightCoroutine);
        }
        
        borderHighLightCoroutine = StartCoroutine(borderHighLight(1));
    }

    /// <summary>
    /// 关闭边缘高亮
    /// </summary>
    public void DisableHighLight()
    {
        isLighting = false;
        
        if (borderHighLightCoroutine != null)
        {
            StopCoroutine(borderHighLightCoroutine);
        }
        
        borderHighLightCoroutine = StartCoroutine(borderHighLight(0));
    }
}
