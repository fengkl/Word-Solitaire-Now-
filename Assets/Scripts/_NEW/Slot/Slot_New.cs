using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Slot_New : MonoBehaviour
{
    // 初始数据
    public List<string> cardInfos;
    
    public int slotId;
    public SlotType slotType;
    public Stack<CardActor> cards = new Stack<CardActor>(); // 卡牌栈
    
    [Header("计算判定数据")]
    public Vector3 keyPos; // 关键位置信息，用于计算拖动卡牌时的相对位置
    public float slotWidth; // 卡槽的判定宽度
    public float slotHeight; // 卡槽的判定高度
    
    [Header("边缘高亮相关参数")] 
    public float borderHighLightInOutTime; // 边缘高亮进入退出时间
    public bool isLighting; // 是否高亮
    public Image highLightBorder; // 边缘高亮渲染器
    private Coroutine borderHighLightCoroutine; // 改变边缘高亮协程
    
    [Header("测试用")]
    public List<CardActor> testCards = new List<CardActor>();

    private void Start()
    {
        keyPos = (transform as RectTransform).position;
        slotWidth = (transform as RectTransform).rect.width;
        slotHeight = (transform as RectTransform).rect.height;
    }

    private void Update()
    {
        testCards = new List<CardActor>(cards);
    }
    
    /// <summary>
    /// 发牌时添加卡牌
    /// </summary>
    public void AddCard(CardActor card)
    {
        card.currentSlot = this; // 设置当前所在的卡槽
        cards.Push(card); // 入栈
        UpdateCardsPos(); // 更新卡牌位置
    }
    
    /// <summary>
    /// 拖动时添加卡牌
    /// </summary>
    public Sequence AddCard(Stack<CardActor> cards)
    {
        foreach (var card in cards)
        {
            card.currentSlot = this; // 设置当前所在的卡槽
            this.cards.Push(card); // 入栈
        }
        return UpdateCardsPos(); // 更新卡牌位置
    }
    
    #region 边缘高亮
    
    /// <summary>
    /// 改变边缘高亮协程
    /// </summary>
    /// <returns></returns>
    private IEnumerator borderHighLight(float alpha)
    {
        Color start = highLightBorder.color;
        Color target = new Color(highLightBorder.color.r, highLightBorder.color.g, highLightBorder.color.b, alpha);

        float timer = 0;

        while (timer < borderHighLightInOutTime)
        {
            timer += Time.deltaTime;
            highLightBorder.color = Color.Lerp(start, target, timer / borderHighLightInOutTime);
            yield return null;
        }
    }

    /// <summary>
    /// 启用边缘高亮
    /// </summary>
    public void EnableHighLight()
    {
        if (isLighting) return;
        
        isLighting = true;

        // 更新位置
        UpdateKeyPosAndHeight();

        // 设置边缘高亮位置
        highLightBorder.rectTransform.position = keyPos;
        // 设置高度
        Vector2 sizeDelta = highLightBorder.rectTransform.sizeDelta;
        sizeDelta.y = slotHeight;
        highLightBorder.rectTransform.sizeDelta = sizeDelta;
        
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
        if (!isLighting) return;
        
        isLighting = false;
        
        if (borderHighLightCoroutine != null)
        {
            StopCoroutine(borderHighLightCoroutine);
        }
        
        borderHighLightCoroutine = StartCoroutine(borderHighLight(0));
    }
    
    #endregion

    /// <summary>
    /// 更新卡牌位置
    /// </summary>
    public virtual Sequence UpdateCardsPos() { return null; }
    
    /// <summary>
    /// 更新keyPos和slotHeight
    /// </summary>
    public virtual void UpdateKeyPosAndHeight() { }


    public virtual void LoadGame(List<string> cardNames)
    {
        // 根据保存的卡牌顺序添加卡牌
        for (int i = cardNames.Count - 1; i >= 0; i--)
        {
            string cardName = cardNames[i];
            if (GameDataUtils.Instance.cardActors.ContainsKey(cardName))
            {
                CardActor card = GameDataUtils.Instance.cardActors[cardName];
                card.currentSlot = this; // 设置当前所在的卡槽
                card.transform.SetAsLastSibling();
                // 如果当前卡牌不是【盖着】【不是栈顶卡牌】且【文字未缩放过】
                if (!card.isFaceDown && i != 0 && !card.isTextScale)
                {
                    card.MovingCardTextToSmall();
                }
                cards.Push(card); // 入栈
            }
        }
    }
}
