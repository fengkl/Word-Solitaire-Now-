using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SlotType
{
    StackArea, // 堆叠区卡槽
    BaseArea // 基础区卡槽
}

public class Slot : MonoBehaviour
{
    public SlotType slotType;
    public Stack<Card> allCards = new Stack<Card>(); // 所有卡牌
    public Stack<CardGroup> allCardGroups = new Stack<CardGroup>(); // 卡组
    public bool isOccupied; // 是否被占用

    
    [Header("边缘高亮相关参数")]
    public float borderHighLightInOutTime; // 边缘高亮进入退出时间
    public bool isLighting; // 是否高亮
    public SpriteRenderer borderRenderer; // 边缘高亮渲染器
    private Coroutine borderHighLightCoroutine; // 改变边缘高亮协程
    
    public List<Card> testCards = new List<Card>(); // 存储的卡牌
    public List<CardGroup> testGroups = new List<CardGroup>(); // 存储的牌组

    void Update()
    {
        testCards = allCards.ToList();
        testGroups = allCardGroups.ToList();
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
