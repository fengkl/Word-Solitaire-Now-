using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class CardGroup : MonoBehaviour
{
    [Header("牌组信息")]
    public List<Card> cards = new List<Card>(); // 牌组列表
    public CategoryData category; // 牌组分类
    public int cardCount; // 卡牌数量
    public float cardGroupHigh; // 牌组高度
    public float gapSize; // 两牌之间的间距

    public BoxCollider2D bc; // 碰撞器
    
    [Header("点击缩放参数")] 
    public float originScale; // 原始缩放
    public float targetScale; // 鼠标点击缩放
    public float duration; // 持续时间
    public Coroutine changeScaleCoroutine; // 缩放协程
    
    [Header("拖拽参数")] 
    public Vector3 originPos; // 原始位置
    public Vector3 offset; // 偏移量
    
    [Header("射线检测参数")]
    public float rayCastRadius; // 射线检测半径
    public float rayCastDistance; // 射线检测距离
    
    [Header("位移参数")]
    public float moveDuration; // 移动到卡槽的时间
    public bool isMoving; // 是否在移动
    
    [Header("选取相关")]
    public Slot currentSlot; // 当前所在卡槽
    public Slot currentSelectSlot; // 当前选择的卡槽
        
    [Header("拿起卡牌相关")]
    public bool canSelected; // 是否可以选中
    public bool isSelected; // 是否被选中
    public CardGroup CurrentPickedCardGroup => GameManager.Instance.currentPickedCardGroup; // 当前拿着的牌组

    public void Start()
    {
        bc = GetComponent<BoxCollider2D>();
        
        Init();
    }

    public void Update()
    {
        FindNearestSlot();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private void Init()
    {
        Card[] cards = GetComponentsInChildren<Card>();

        foreach (var card in cards)
        {
            AddCard(card);
        }

        cardCount = cards.Length;
        category = cards[0].category; // 初始化种类
        canSelected = true;
        
        ChangeBoxColliderSize(); // 设置碰撞器的大小
    }

    /// <summary>
    /// 添加卡牌到牌组
    /// </summary>
    /// <param name="card"></param>
    public void AddCard(Card card)
    {
        // 调整新增卡牌的图层先后
        if (cards.Count > 0)
        {
            card.cardSurfaceSr.sortingOrder = cards[cards.Count - 1].cardSurfaceSr.sortingOrder - 3;
            if (card.cardBorderSr != null)
            {
                card.cardBorderSr.sortingOrder = cards[cards.Count - 1].cardBorderSr.sortingOrder - 3;
            }
            card.cardCanvas.sortingOrder = cards[cards.Count - 1].cardCanvas.sortingOrder - 3;
        }

        card.parent = gameObject;
        cards.Add(card);
    }

    /// <summary>
    /// 修改碰撞器尺寸，使其能包裹住所有子物体（这部分问的ai，注释是自己加的，以便理解）
    /// </summary>
    private void ChangeBoxColliderSize()
    {
        // 创建一个包围盒
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        
        // 遍历所有卡牌
        foreach (Card card in cards)
        {
            // 获取卡牌的边框，并扩展当前的包围盒边界，使其能包裹住所有卡牌
            bounds.Encapsulate(card.cardSurfaceSr.bounds);
        }
        
        // 设置碰撞器的大小
        bc.size = bounds.size; // 设置碰撞器尺寸
    }
    
    #region 鼠标操作相关

    private void OnMouseDown()
    {
        // 牌组移动和无法选中时无法操作
        if (isMoving || !canSelected) return;
        
        isSelected = true; // 设置为被拿起
        originPos = transform.position; // 记录原始位置

        foreach (Card card in cards)
        {
            // 调整图层
            card.cardSurfaceSr.sortingOrder += 999;
            if (card.cardBorderSr != null)
            {
                card.cardBorderSr.sortingOrder += 999;
            }
            card.cardCanvas.sortingOrder += 999;
            
            card.StartChangeScale(targetScale); // 变大一点
        }
        
        GetOffset(); // 获得中心与鼠标的偏移量
        PickUpCardGroup(); // 拿起牌组
    }

    private void OnMouseUp()
    {
        // 卡牌移动和无法选中时无法操作
        if (isMoving || !canSelected) return;
        
        foreach (Card card in cards)
        {
            card.StartChangeScale(originScale); // 变回原来的大小
        }
        
        PlaceCardGroup(); // 放置卡牌
        
        // 设置为被放下
        isSelected = false;
    }

    private void OnMouseDrag()
    {
        if (isSelected && canSelected)
        {
            Vector3 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition); // 屏幕坐标转世界坐标
            transform.position = currentPos + offset;
        }
        
    }

    #endregion
    
    #region 点击时缩放相关

    /// <summary>
    /// 获取鼠标与卡牌之间的偏移量
    /// </summary>
    public void GetOffset()
    {
        offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
    
    #endregion

    #region 拿起放下牌组相关
    
    /// <summary>
    /// 拿起牌组
    /// </summary>
    private void PickUpCardGroup()
    {
        // 如果有绑定的卡槽，取出卡槽中的卡组
        if (currentSlot != null)
        {
            // 从卡槽栈中取出对应的牌组
            CardGroup cardGroup = currentSlot.allCardGroups.Pop();
            for (int i = 0; i < cardGroup.cards.Count; i++)
            {
                currentSlot.allCards.Pop();
            }
            
            GameManager.Instance.currentPickedCardGroup = cardGroup; // 设置拿着的牌组为当前牌组
        }
        // 如果没有绑定的卡槽，则直接赋值
        else
        {
            GameManager.Instance.currentPickedCardGroup = this; // 设置拿着的牌组为当前牌组
        }
    }
    
    /// <summary>
    /// 放下牌组
    /// </summary>
    private void PlaceCardGroup()
    {
        // 如果【被选中】，则放置卡牌
        if (isSelected)
        {
            // 重置上一个卡槽的状态
            if (currentSlot != null)
            {
                if (currentSlot.allCardGroups.Count == 0)
                {
                    currentSlot.isOccupied = false; // 取消占据状态
                }
            }
            
            ExecutePlaceCardGroup(); // 执行放置逻辑
        }
    }

    /// <summary>
    /// 执行放置逻辑
    /// </summary>
    private void ExecutePlaceCardGroup()
    {
        // 如果找到了可用卡槽，则设置为绑定的卡槽
        if (currentSelectSlot != null)
        {
            // 如果为叠牌区卡槽
            if (currentSelectSlot.slotType == SlotType.StackArea)
            {
                // 如果【卡槽为空】或【顶部卡牌为文字卡】
                if (currentSelectSlot.allCards.Count == 0 || currentSelectSlot.allCards.Peek().cardType == CardType.Character)
                {
                    currentSlot = currentSelectSlot;
                }
                // 如果【顶部卡牌为分类卡】
                else if (currentSelectSlot.allCards.Peek().cardType == CardType.Category)
                {
                    print("文字卡不能放置在分类卡上");
                }
            }
            // 如果为基础区卡槽
            else if (currentSelectSlot.slotType == SlotType.BaseArea)
            {
                // 如果【卡槽为空】
                if (currentSelectSlot.allCards.Count == 0)
                {
                    // 如果【当前拿着的牌组中包含分类卡】
                    if (CurrentPickedCardGroup.cards[0].cardType == CardType.Category)
                    {
                        currentSlot = currentSelectSlot;
                    }
                    else
                    {
                        print("文字卡不能放在基础区里");
                    }
                }
                // 如果【卡槽不为空】
                else
                {
                    // 如果【当前拿着的牌组包含分类卡】
                    if (CurrentPickedCardGroup.cards[0].cardType == CardType.Character)
                    {
                        // 如果【文字卡与当前分类相同】
                        if (CurrentPickedCardGroup.cards[0].cardType == currentSelectSlot.allCards.Peek().cardType)
                        {
                            currentSlot = currentSelectSlot;
                        }
                        else
                        {
                            print("此文字卡与该分类不符");
                        }
                    }
                    else
                    {
                        print("分类卡只能放在空的基础区");
                    }
                }
                
            }
        }

        // 如果有绑定卡槽
        if (currentSlot != null)
        {
            // 设置卡槽为被占据状态
            currentSlot.isOccupied = true;
            
            // 将当前卡牌压入卡槽栈
            for (int i = CurrentPickedCardGroup.cards.Count - 1; i >= 0; i--)
            {
                Card card = CurrentPickedCardGroup.cards[i];
                currentSlot.allCards.Push(card);
            }
            
            // 如果为叠牌区卡槽
            if (currentSlot.slotType == SlotType.StackArea)
            {
                // 移动到叠牌区
                MovingCardToStackAreaSlot(currentSlot as StackAreaSlot);
            }
            // 如果为基础区卡槽
            else if (currentSlot.slotType == SlotType.BaseArea)
            {
                // 移动到基础区
                MovingCardToBaseAreaSlot(currentSlot as BaseAreaSlot);
            }
        }
        else
        {
            MoveCardToOriginalPos(); // 移动到原位
        }
    }
    
    #endregion

    #region 移动动画相关
    
    /// <summary>
    /// 移动卡组至叠牌区卡槽
    /// </summary>
    public void MovingCardToStackAreaSlot(StackAreaSlot stackAreaSlot)
    {
        isMoving = true;
        
        int count = stackAreaSlot.allCards.Count - 1; // 计数器
        
        // 未达到执行卡牌压缩的数量
        if (count < stackAreaSlot.maxCardCount)
        {
            // 创建序列动画
            Sequence sequence = DOTween.Sequence();
            
            // 遍历卡组中的所有卡牌,设置位置
            foreach (Card card in stackAreaSlot.allCards)
            {
                // 先与父物体卡组解绑
                card.transform.parent = null;
                
                // 获取目标位置
                Vector3 cardTarget = stackAreaSlot.transform.position + new Vector3(0, -count * stackAreaSlot.gapSize, -1);

                // 移动卡牌位置
                sequence.Join(card.transform.DOMove(cardTarget, moveDuration))
                    .SetEase(Ease.InOutQuad);
                
                // 如果当前卡牌不是栈顶卡牌，且文字未缩放过
                if (card != stackAreaSlot.allCards.Peek() && !card.isTextScale)
                {
                    card.MovingCardText();
                }

                count--;
            }

            sequence.AppendCallback(() =>
            {
                // 合并牌组
                if (stackAreaSlot.allCardGroups.Count > 0)
                {
                    CardGroup group1 = CurrentPickedCardGroup;
                    CardGroup group2 = stackAreaSlot.allCardGroups.Pop();

                    CardGroup newGroup = MergeGroup(group1, group2);
                    stackAreaSlot.allCardGroups.Push(newGroup);
                }
                else
                {
                    stackAreaSlot.allCardGroups.Push(CurrentPickedCardGroup);
                }

                MovingCardGroupPosInStackAreaSlot(stackAreaSlot); // 移动牌组位置
            });
        }
    }

    /// <summary>
    /// 移动牌组物体位置至能够覆盖牌组内卡牌（叠牌区）
    /// </summary>
    private void MovingCardGroupPosInStackAreaSlot(StackAreaSlot stackAreaSlot)
    {
        // 用牌组最前和最后两张牌的位置计算牌组正中间的位置
        float groupPos = (cards[0].transform.position.y - cards[cards.Count - 1].transform.position.y) / 2;
        // 牌组的目标位置
        Vector3 groupTarget = stackAreaSlot.transform.position + new Vector3(0, groupPos, -1);

        // 移动牌组位置
        transform.position = groupTarget;
        
        foreach (Card card in cards)
        {
            // 调整图层
            card.cardSurfaceSr.sortingOrder -= 999;
            if (card.cardBorderSr != null)
            {
                card.cardBorderSr.sortingOrder -= 999;
            }
            card.cardCanvas.sortingOrder -= 999;
        }
        
        // 重新绑定父子关系
        foreach (Card card in stackAreaSlot.allCards)
        {
            card.transform.parent = card.parent.transform;
        }
            
        ChangeBoxColliderSize(); // 重新设置碰撞器大小
        
        isMoving = false;
    }
    
    /// <summary>
    /// 移动卡组至基础区卡槽
    /// </summary>
    public void MovingCardToBaseAreaSlot(BaseAreaSlot baseAreaSlot)
    {
        isMoving = true;

        if (CurrentPickedCardGroup.cards.Count == 1)
        {
            // 创建序列动画
            Sequence sequence = DOTween.Sequence();

            Card card = CurrentPickedCardGroup.cards[0];
            
            // 先与父物体卡组解绑
            card.transform.parent = null;
            
            // 获取目标位置
            Vector3 cardTarget = currentSlot.transform.position;
            
            // 移动卡牌位置
            sequence.Join(card.transform.DOMove(cardTarget, moveDuration))
                .SetEase(Ease.InOutQuad);
        
        
            sequence.AppendCallback(() =>
            {
                // 合并牌组
                if (baseAreaSlot.allCardGroups.Count > 0)
                {
                    CardGroup group1 = CurrentPickedCardGroup;
                    CardGroup group2 = baseAreaSlot.allCardGroups.Pop();
            
                    CardGroup newGroup = MergeGroup(group1, group2);
                    baseAreaSlot.allCardGroups.Push(newGroup);
                }
                else
                {
                    baseAreaSlot.allCardGroups.Push(CurrentPickedCardGroup);
                }
            
                MovingCardGroupPosInBaseAreaSlot(baseAreaSlot); // 移动牌组位置
            });
        
        }
    }
    
    /// <summary>
    /// 移动牌组物体位置至能够覆盖牌组内卡牌（基础区）
    /// </summary>
    private void MovingCardGroupPosInBaseAreaSlot(BaseAreaSlot baseAreaSlot)
    {
        // 牌组的目标位置
        Vector3 groupTarget = baseAreaSlot.transform.position;

        // 移动牌组位置
        transform.position = groupTarget;
        
        foreach (Card card in cards)
        {
            // 调整图层
            card.cardSurfaceSr.sortingOrder -= 999;
            if (card.cardBorderSr != null)
            {
                card.cardBorderSr.sortingOrder -= 999;
            }
            card.cardCanvas.sortingOrder -= 999;
        }
        
        // 重新绑定父子关系
        foreach (Card card in baseAreaSlot.allCards)
        {
            card.transform.parent = card.parent.transform;
        }
            
        ChangeBoxColliderSize(); // 重新设置碰撞器大小
        
        isMoving = false;
    }

    /// <summary>
    /// 移动卡牌至原位
    /// </summary>
    public void MoveCardToOriginalPos()
    {
        // 获取目标位置
        Vector3 target = originPos;

        // 如果有绑定的卡槽，则回到卡槽位置，否则回到原始位置
        transform.DOMove(target, moveDuration)
            .SetEase(Ease.InOutQuad)
            .OnStart(() => isMoving = true)
            .OnComplete(() =>
            {
                isMoving = false;
            });
        
        foreach (Card card in cards)
        {
            // 调整图层
            card.cardSurfaceSr.sortingOrder -= 999;
            if (card.cardBorderSr != null)
            {
                card.cardBorderSr.sortingOrder -= 999;
            }
            card.cardCanvas.sortingOrder -= 999;
        }
    }

    #endregion
    
    #region 检测附近卡槽或卡牌相关
    
    /// <summary>
    /// 找最近的卡槽或卡牌
    /// </summary>
    public void FindNearestSlot()
    {
        if (!isSelected) return;
        
        // 获取附近所有卡槽
        RaycastHit2D[] raycastHitsSlots = Physics2D.CircleCastAll(cards[0].transform.position, rayCastRadius,
            Vector2.up, rayCastDistance * (cards.Count - 1), LayerMask.GetMask("StackAreaSlot", "BaseAreaSlot"));
        
        Slot nearestSlot = null;
        float nearestDistance = float.MaxValue;
    
        // 寻找最近的卡槽
        foreach (RaycastHit2D slot in raycastHitsSlots)
        {
            // 忽略被占据的卡槽
            if (slot.collider.gameObject.GetComponent<Slot>().isOccupied) continue;
            
            float currentDistance = Vector3.Distance(slot.collider.transform.position, transform.position);
            
            if (currentDistance < nearestDistance)
            {
                nearestSlot = slot.collider.GetComponent<Slot>();
                nearestDistance = currentDistance;
            }
        }
        
        currentSelectSlot = nearestSlot;
        
        // 如果没有找到卡槽，则寻找最近的牌组
        if (nearestSlot == null)
        {
            // 获取附近所有卡牌
            RaycastHit2D[] raycastHitsCardGroups = Physics2D.CircleCastAll(cards[0].transform.position, rayCastRadius,
                Vector2.down, rayCastDistance * (cards.Count - 1), LayerMask.GetMask("CardGroup"));
            
            CardGroup nearestCardGroup = null;
            nearestDistance = float.MaxValue;
            
            // 寻找最近的卡牌
            foreach (RaycastHit2D cardGroup in raycastHitsCardGroups)
            {
                // 排除自己的卡牌
                if (cardGroup.collider.GetComponent<CardGroup>() == this) continue;
                
                // 排除不同分类的卡牌
                if (cardGroup.collider.GetComponent<CardGroup>().category != category)
                {
                    print("分类不同");
                    continue;
                }
                
                float currentDistance = Vector3.Distance(cardGroup.collider.transform.position, transform.position);
                
                if (currentDistance < nearestDistance)
                {
                    nearestCardGroup = cardGroup.collider.GetComponent<CardGroup>(); // 找到最顶端的卡牌
                    nearestDistance = currentDistance;
                }
            }
            
            if (nearestCardGroup != null)
            {
                currentSelectSlot = nearestCardGroup.currentSlot;
            }
        }
    }

    #endregion

    # region 卡牌合并相关
    
    /// <summary>
    /// 合并两个牌组
    /// </summary>
    /// <param name="group1"></param>
    /// <param name="group2"></param>
    private CardGroup MergeGroup(CardGroup group1, CardGroup group2)
    {
        // 将group2的卡牌绑定到group1中
        foreach (Card card in group2.cards)
        {
            AddCard(card);
        }
        
        Destroy(group2.gameObject); //销毁空牌组

        return group1;
    }
    
    #endregion
}
