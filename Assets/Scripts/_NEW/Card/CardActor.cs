using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardActor : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("卡牌基本信息")] 
    public CardType cardType; // 卡牌类型
    public string cardCategory; // 卡牌分类
    public string cardName; // 卡牌名称
    public Slot_New currentSlot; // 当前所在槽位
    public Move dragMove; // 暂存拖拽移动的数据

    [Header("卡牌图片和文字")] 
    public GameObject cardFront; // 卡牌正面物体集合
    public Image cardBack; // 卡背
    public Image cardFace; // 卡面
    public Image cardBorder; // 卡边
    public Text nameText; // 名称文本
    public bool useSprite; // 是否使用精灵
    public Image cardImage; // 卡牌精灵
    
    [Header("点击缩放参数")] 
    public float originScale; // 原始大小
    public float duration; // 持续时间
    public Coroutine changeScaleCoroutine; // 缩放协程

    [Header("卡面文字和移动相关")] 
    public float textScaleValue; // 缩放值
    public float textMoveDistance; // 文字移动的距离
    public float cardMoveDistance; // 卡牌移动的距离
    public float originalScaleValue; // 原始缩放值
    public float moveDuration; // 文字移动持续时间
    public bool isTextScale; // 是否缩放了文字
    
    [Header("翻牌相关")]
    public float flipDuration; // 翻牌持续时间
    public bool isFaceDown; // 是否盖着牌

    [Header("卡牌操作")] 
    public bool isMoving; // 是否正在移动
    public bool canSelected; // 是否可以被选中
    
    [Header("拖拽参数")] 
    public Vector3 offset; // 偏移量

    [Header("红点")] 
    public GameObject redPoint;
    public Text redCountText;
    
    private Sequence moveSequence; // 移动序列

    #region 鼠标操作相关

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isMoving || !canSelected) return;

        int count = 0;
        CardActor redCard = null;

        // 让所有被选择的牌放大
        foreach (var card in currentSlot.cards)
        {
            // 如果为可操作卡槽，则全部放大
            if (currentSlot as OperateSlot != null)
            {
                if (!card.isFaceDown)
                {
                    card.StartChangeScale(originScale * 1.05f);

                    if (card.cardType == CardType.Character)
                    {
                        count++;
                        redCard = card;
                    }
                }
            }
            // 如果为取牌卡槽，则只放大第一张牌
            else if (currentSlot as TakeSlot != null)
            {
                StartChangeScale(originScale * 1.05f);
                break;
            }
        }

        // 判断红点是否启用
        if (count > 1 && redCard != null)
        {
            redCard.redCountText.text = count.ToString();
            redCard.redPoint.SetActive(true);
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        // 让所有被选择的牌恢复原始大小
        foreach (var card in currentSlot.cards)
        {
            card.StartChangeScale(originScale);
            if (card.redPoint != null)
            {
                card.redPoint.SetActive(false);
            }
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isMoving || !canSelected)
        {
            eventData.pointerDrag = null; // 取消拖拽注册，防止卡牌执行进入卡槽的动画时点击拖拽会出现问题
            return;
        }
        
        // 将卡牌从卡槽中取出，并存到dragMove中
        dragMove = new Move(currentSlot, cardCategory);
        
        // 取出卡槽中所有需要被拖动的牌
        while (currentSlot.cards.Count != 0)
        {
            CardActor card = currentSlot.cards.Peek();
            
            // 如果为可操作卡槽中的卡牌，则全部拿取
            if (currentSlot as OperateSlot != null)
            {
                if (!card.isFaceDown)
                {
                    // 如果存在分类卡，则记录
                    if (card.cardType == CardType.Category)
                    {
                        dragMove.categoryCard = card as CategoryCardActor;
                    }
                    
                    dragMove.cards.Push(currentSlot.cards.Pop());
                    dragMove.cardCount++;
                }
                else
                {
                    break;
                }
            }
            // 如果为取牌卡槽中的卡牌，则只拿一张
            else if (currentSlot as TakeSlot != null)
            {
                // 如果存在分类卡，则记录
                if (card.cardType == CardType.Category)
                {
                    dragMove.categoryCard = card as CategoryCardActor;
                }
                
                dragMove.cards.Push(currentSlot.cards.Pop());
                dragMove.cardCount++;
                break;
            }
        }

        // 设置所有拖拽卡牌与卡牌中心的偏移值，并调整卡牌层级
        foreach (var card in dragMove.cards)
        {
            card.GetOffset(eventData);
        }
        
        // 移动到moveGroup
        dragMove.MoveToMoveGroup();
        
        // 如果从取牌区离开，则更新取牌区的牌的位置
        if (currentSlot as TakeSlot != null)
        {
            (currentSlot as TakeSlot).UpdateCardsPos();
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        // 设置偏移值
        foreach (var card in dragMove.cards)
        {
            card.SetOffset(eventData);
        }
        
        // 计算获取附近可用卡槽
        List<Slot_New> allSlots = GameDataUtils.Instance.GetAllSlots();
        dragMove.keyPos = dragMove.cards.Peek().transform.position;

        // 先将高亮全部禁用
        foreach (var slot in allSlots)
        {
            slot.DisableHighLight();
        }

        // 启用检测到的第一个卡槽的高亮
        foreach (var slot in allSlots)
        {
            if (slot == currentSlot) continue;
            
            // 如果根据宽高检测到了可用卡槽，且【卡槽中有牌 且 第一张牌的类型与自身类型相同 且 第一张牌不为分类卡】或【卡槽为空】
            if (Math.Abs(dragMove.keyPos.x - slot.keyPos.x) < slot.slotWidth &&
                Math.Abs(dragMove.keyPos.y - slot.keyPos.y) < slot.slotHeight &&
                ((slot.cards.Count != 0 && dragMove.category == slot.cards.Peek().cardCategory) || slot.cards.Count == 0))
            {
                // 如果OperateSlot顶部第一张卡是分类卡，则跳过
                if (slot as OperateSlot != null && slot.cards.Count != 0 && slot.cards.Peek().cardType == CardType.Category) continue;
                
                // 如果在双方都没有分类卡的情况下进入了TargetSlot，则跳过
                if (slot as TargetSlot != null && dragMove.categoryCard == null && (slot as TargetSlot).categoryCard == null) continue;
                
                // 启用边缘高亮
                slot.EnableHighLight();
                break;
            }
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        // 获取附近可用卡槽
        List<Slot_New> allSlots = GameDataUtils.Instance.GetAllSlots();
        
        dragMove.keyPos = dragMove.cards.Peek().transform.position;

        foreach (var slot in allSlots)
        {
            if (slot == currentSlot) continue;
            
            // 如果根据宽高检测到了可用卡槽，且【卡槽中有牌 且 第一张牌的类型与自身类型相同 且 第一张牌不为分类卡】或【卡槽为空】
            if (Math.Abs(dragMove.keyPos.x - slot.keyPos.x) < slot.slotWidth &&
                Math.Abs(dragMove.keyPos.y - slot.keyPos.y) < slot.slotHeight &&
                ((slot.cards.Count != 0 && dragMove.category == slot.cards.Peek().cardCategory) || slot.cards.Count == 0))
            {
                // 如果OperateSlot顶部第一张卡是分类卡，则跳过
                if (slot as OperateSlot != null && slot.cards.Count != 0 && slot.cards.Peek().cardType == CardType.Category) continue;
                
                // 如果在双方都没有分类卡的情况下进入了TargetSlot，则跳过
                if (slot as TargetSlot != null && dragMove.categoryCard == null && (slot as TargetSlot).categoryCard == null) continue;
                
                // 如果选中的卡槽为目标卡槽，且还没有分类卡，则设置分类卡
                if (slot as TargetSlot != null && (slot as TargetSlot).categoryCard == null)
                {
                    (slot as TargetSlot).categoryCard = dragMove.categoryCard;
                }
                
                dragMove.to = slot;
                
                // 翻开先前的卡槽的第一张牌
                if (currentSlot.cards.Count != 0)
                {
                    currentSlot.cards.Peek().FlipCard();
                }
                
                moveSequence = slot.AddCard(dragMove.cards); // 将卡牌添加到目标卡槽
                moveSequence.OnComplete(() => 
                {
                    GameStateManager.Instance.SaveCurrentGameState();
                    dragMove.MoveToCardGroup();
                });
                
                slot.DisableHighLight(); // 禁用边缘高亮
                break;
            }
        }
        
        // 如果没有找到可用卡槽
        if (dragMove.to == null)
        {
            // 如果为取牌区的卡牌
            if (dragMove.from as TakeSlot != null)
            {
                moveSequence = (dragMove.from as TakeSlot).AddCard(dragMove.cards);
            }
            // 如果为叠牌区的卡牌
            else if (dragMove.from as OperateSlot != null)
            {
                moveSequence = (dragMove.from as OperateSlot).AddCard(dragMove.cards);
            }
            
            moveSequence.OnComplete(() => 
            {
                GameStateManager.Instance.SaveCurrentGameState();
                dragMove.MoveToCardGroup();
            });
        }
        
        // 让所有被选择的牌恢复原始大小
        foreach (var card in dragMove.cards)
        {
            card.StartChangeScale(originScale);
            if (card.redPoint != null)
            {
                card.redPoint.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 获取偏移值
    /// </summary>
    public void GetOffset(PointerEventData eventData)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        worldPos.z = transform.position.z;
        offset = transform.position - worldPos;
    }

    /// <summary>
    /// 设置偏移值
    /// </summary>
    public void SetOffset(PointerEventData eventData)
    {        
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        worldPos.z = transform.position.z;
        transform.position = worldPos + offset;
    }

    #endregion
    
    #region 点击时缩放的相关方法
    
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

    #region 卡面文字和移动相关
    
    /// <summary>
    /// 移动牌面文字并变小
    /// </summary>
    public void MovingCardTextToSmall(int customDuration = -1)
    {
        if (isTextScale) return;
        
        isTextScale = true;

        // 隐藏边框
        cardBorder.enabled = false;
        
        // 创建序列动画
        Sequence sequence = DOTween.Sequence();

        // 缩放和移动文字
        sequence.Join(nameText.transform.DOScale(textScaleValue, customDuration != -1 ? customDuration : moveDuration));
        sequence.Join(nameText.transform.DOBlendableLocalMoveBy(new Vector3(0, textMoveDistance, 0), customDuration != -1 ? customDuration : moveDuration));
    }
    
    /// <summary>
    /// 移动牌面文字并变大
    /// </summary>
    public void MovingCardTextToBig()
    {
        if (!isTextScale) return;
        
        isTextScale = false;

        // 显示边框
        cardBorder.enabled = true;
        
        // 创建序列动画
        Sequence sequence = DOTween.Sequence();

        // 缩放和移动文字
        sequence.Join(nameText.transform.DOScale(originalScaleValue, moveDuration));
        sequence.Join(nameText.transform.DOBlendableLocalMoveBy(new Vector3(0, -textMoveDistance, 0), moveDuration));
    }

    /// <summary>
    /// 移动牌位置
    /// </summary>
    public void MovingCardPos(int customDuration = -1)
    {
        transform.DOMoveY(currentSlot.transform.position.y + cardMoveDistance, customDuration != -1 ? customDuration : moveDuration);
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
        
        sequence.Append(cardBack.transform.DOScaleX(0, customDuration != -1 ? customDuration : flipDuration));
        sequence.Append(cardFront.transform.DOScaleX(1, customDuration != -1 ? customDuration : flipDuration));
        sequence.AppendCallback(() =>
        {
            if (customDuration == -1)
            {
                canSelected = true;
            }
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
        sequence.Append(cardBack.transform.DOScaleX(1, flipDuration));
        sequence.AppendCallback(() =>
        {
            canSelected = false;
        });
    }
    
    #endregion
}
