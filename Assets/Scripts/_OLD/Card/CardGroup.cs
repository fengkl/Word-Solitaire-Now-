using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class CardGroup : MonoBehaviour
{
    [Header("牌组信息")]
    public List<Card> cards = new List<Card>(); // 牌组列表
    public string category; // 牌组分类
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
    
    [Header("红点计数")]
    public GameObject redPoint; // 红点
    public TextMeshProUGUI countText; // 数量文本
    public float redPointUpDistance; // 红点上升距离

    public bool IsInitializing => GameManager.Instance.isInitializing; // 是否初始化完成

    public void Update()
    {
        FindNearestSlot();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        bc = GetComponent<BoxCollider2D>();
        
        Card[] cards = GetComponentsInChildren<Card>();

        foreach (var card in cards)
        {
            AddCard(card);
        }

        cardCount = cards.Length;
        category = cards[0].cardCategory; // 初始化种类
        canSelected = false;
        
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
            card.cardSurfaceSr.sortingOrder = cards[cards.Count - 1].cardSurfaceSr.sortingOrder - 5;
            if (card.cardBorderSr != null)
            {
                card.cardBorderSr.sortingOrder = cards[cards.Count - 1].cardBorderSr.sortingOrder - 5;
            }
            card.cardCanvas.sortingOrder = cards[cards.Count - 1].cardCanvas.sortingOrder - 5;
            card.cardBackSr.sortingOrder = cards[cards.Count - 1].cardBackSr.sortingOrder - 5;
        }

        card.parent = gameObject;
        cards.Add(card);
    }

    /// <summary>
    /// 修改碰撞器尺寸，使其能包裹住所有子物体（这部分问的ai，注释是自己加的，以便理解）
    /// </summary>
    public void ChangeBoxColliderSize()
    {
        // 创建一个包围盒
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        
        // 遍历所有卡牌
        foreach (Card card in cards)
        {
            // 获取卡牌的边框，并扩展当前的包围盒边界，使其能包裹住所有卡牌
            bounds.Encapsulate(card.cardSurfaceSr.bounds);
            bounds.Encapsulate(card.cardBackSr.bounds);
        }
        
        // 设置碰撞器的大小
        bc.size = bounds.size;
    }
    
    #region 鼠标操作相关

    private void OnMouseDown()
    {
        // 【牌组移动】【无法选中】【处于初始化】时无法操作
        if (isMoving || !canSelected || IsInitializing) return;
        
        isSelected = true; // 设置为被拿起
        GameManager.Instance.isMovingCard = true; // 设置为正在移动卡牌
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
        
        DisplayCount(); // 显示数量
        GetOffset(); // 获得中心与鼠标的偏移量
        PickUpCardGroup(); // 拿起牌组
    }

    private void OnMouseUp()
    {
        // 【牌组移动】【无法选中】【处于初始化】时无法操作
        if (isMoving || !canSelected || IsInitializing) return;
        
        foreach (Card card in cards)
        {
            card.StartChangeScale(originScale); // 变回原来的大小
        }
        
        // 关闭边缘高亮
        if (currentSelectSlot != null)
        {
            currentSelectSlot.DisableHighLight();
        }
        
        redPoint.SetActive(false);
        PlaceCardGroup(currentSelectSlot); // 放置卡牌
        
        isSelected = false; // 设置为被放下
        GameManager.Instance.isMovingCard = false;
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
                if (currentSlot.allCards.Count != 0)
                {
                    currentSlot.allCards.Pop();
                }
            }

            if (currentSlot as TakeAreaSlot)
            {
                (currentSlot as TakeAreaSlot).MoveCardGroupsToUp();
            }
            
            // GameManager.Instance.currentPickedCardGroup = cardGroup; // 设置拿着的牌组为当前牌组
        }
        // // 如果没有绑定的卡槽，则直接赋值
        // else
        // {
        //     GameManager.Instance.currentPickedCardGroup = this; // 设置拿着的牌组为当前牌组
        // }
    }
    
    /// <summary>
    /// 放下牌组
    /// </summary>
    public void PlaceCardGroup(Slot selectedSlot)
    {
        // 重置上一个卡槽的状态
        if (currentSlot != null)
        {
            if (currentSlot.allCardGroups.Count == 0)
            {
                currentSlot.isOccupied = false; // 取消占据状态
            }
        }
        
        ExecutePlaceCardGroup(selectedSlot); // 执行放置逻辑
    }

    /// <summary>
    /// 执行放置逻辑
    /// </summary>
    private void ExecutePlaceCardGroup(Slot selectedSlot)
    {
        isMoving = true;
        
        // 处理盖牌情况
        if (cards[0].isFaceDown)
        {
            currentSlot = selectedSlot;
        }
        // 如果找到了可用卡槽，则设置为绑定的卡槽
        else if (selectedSlot != null)
        {
            // 如果为叠牌区卡槽
            if (selectedSlot.slotType == SlotType.OperateSlot)
            {
                // 如果【卡槽为空】或【卡槽顶部卡牌为同分类】的同时【卡槽顶部卡牌为文字卡】
                if (selectedSlot.allCards.Count == 0 || 
                    selectedSlot.allCards.Peek().cardCategory == category && selectedSlot.allCards.Peek().cardType == CardType.Character)
                {
                    currentSlot = selectedSlot;
                }
                // 如果【卡槽顶部卡牌为分类卡】
                else if (selectedSlot.allCards.Peek().cardType == CardType.Category)
                {
                    print("文字卡不能放置在分类卡上");
                }
                // 如果【卡槽顶部卡牌为不同分类】
                else if (selectedSlot.allCards.Peek().cardCategory != category)
                {
                    print("你只能叠放同一分类的文字");
                }
            }
            // 如果为基础区卡槽
            else if (selectedSlot.slotType == SlotType.TargetSlot)
            {
                // 如果【卡槽为空】
                if (selectedSlot.allCards.Count == 0)
                {
                    // 如果【牌组中包含分类卡】
                    if (cards[0].cardType == CardType.Category)
                    {
                        currentSlot = selectedSlot;
                    }
                    else
                    {
                        print("文字卡不能放在基础区里");
                    }
                }
                // 如果【卡槽不为空】
                else
                {
                    // 如果【牌组包含分类卡】
                    if (cards[0].cardType == CardType.Character)
                    {
                        // 如果【文字卡与当前分类相同】
                        if (cards[0].cardCategory == selectedSlot.allCards.Peek().cardCategory)
                        {
                            currentSlot = selectedSlot;
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
            // 如果为取牌区卡槽
            else if (selectedSlot.slotType == SlotType.HomeSlot)
            {
                currentSlot = selectedSlot;
            }
        }

        // 如果有绑定卡槽
        if (currentSlot != null)
        {
            // 设置卡槽为被占据状态
            currentSlot.isOccupied = true;
            
            // 将当前卡牌压入卡槽栈
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                currentSlot.allCards.Push(cards[i]);
            }
            
            // 如果为叠牌区卡槽
            if (currentSlot.slotType == SlotType.OperateSlot)
            {
                // 移动到叠牌区
                MovingCardToStackAreaSlot(currentSlot as StackAreaSlot);
            }
            // 如果为基础区卡槽
            else if (currentSlot.slotType == SlotType.TargetSlot)
            {
                // 移动到基础区
                MovingCardToBaseAreaSlot(currentSlot as BaseAreaSlot);
            }
            // 如果为取牌区卡槽
            else if (currentSlot.slotType == SlotType.HomeSlot)
            {
                // 移动到取牌区
                MovingCardToTakeAreaSlot(currentSlot as TakeAreaSlot);
            }
        }
        else
        {
            MoveCardToOriginalPos(); // 移动到原位
        }
    }
    
    #endregion

    #region 移动至叠牌区相关
    
    /// <summary>
    /// 移动卡组至叠牌区卡槽
    /// </summary>
    public void MovingCardToStackAreaSlot(StackAreaSlot stackAreaSlot)
    {
        int count = stackAreaSlot.allCards.Count - 1; // 计数器
        
        // 创建序列动画
        Sequence sequence = DOTween.Sequence();
        
        // 遍历卡组中的所有卡牌,设置位置
        foreach (Card card in stackAreaSlot.allCards)
        {
            // 先与父物体卡组解绑
            card.transform.parent = null;
            
            // 获取目标位置
            Vector3 cardTarget = stackAreaSlot.transform.position + new Vector3(0, -count * stackAreaSlot.gapSize, -(count + 1));

            // 移动卡牌位置
            sequence.Join(card.transform.DOMove(cardTarget, moveDuration))
                .SetEase(Ease.InOutQuad);
            
            // 如果当前卡牌不是【盖着】【不是栈顶卡牌】且【文字未缩放过】
            if (!card.isFaceDown && card != stackAreaSlot.allCards.Peek() && !card.isTextScale)
            {
                card.MovingCardTextToSmall();
            }

            count--;
        }

        sequence.AppendCallback(() =>
        {
            // 合并牌组
            if (stackAreaSlot.allCardGroups.Count > 0 && !stackAreaSlot.allCardGroups.Peek().cards[0].isFaceDown)
            {
                CardGroup group1 = this;
                CardGroup group2 = stackAreaSlot.allCardGroups.Pop();

                CardGroup newGroup = MergeGroup(group1, group2);
                stackAreaSlot.allCardGroups.Push(newGroup);
            }
            else
            {
                stackAreaSlot.allCardGroups.Push(this);
            }

            MovingCardGroupPosInStackAreaSlot(stackAreaSlot); // 移动牌组位置
        });
    }

    /// <summary>
    /// 移动牌组物体位置至能够覆盖牌组内卡牌（叠牌区）
    /// </summary>
    private void MovingCardGroupPosInStackAreaSlot(StackAreaSlot stackAreaSlot)
    {
        // 用牌组最前和最后两张牌的位置计算牌组正中间的位置
        float groupPos = (cards[0].transform.position.y + cards[cards.Count - 1].transform.position.y) / 2;
        // 牌组的目标位置
        Vector3 groupTarget = new Vector3(cards[0].transform.position.x, groupPos, cards[0].transform.position.z);
        
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
    
    #endregion
    
    #region 移动至基础区相关
    
    /// <summary>
    /// 移动卡组至基础区卡槽
    /// </summary>
    public void MovingCardToBaseAreaSlot(BaseAreaSlot baseAreaSlot)
    {
        isMoving = true;

        // 设置卡槽所放置的分类卡和分类
        if (baseAreaSlot.categoryCard == null)
        {
            baseAreaSlot.SetCategoryCard(cards[0] as CategoryCard);
            
            // 处理分类卡与多张文字卡一起进入卡槽时，放入卡槽前微调分类卡的位置
            if (cards.Count > 1)
            {
                baseAreaSlot.categoryCard.transform.position = cards[1].transform.position;
            }
            
            // 调整图层
            baseAreaSlot.categoryCard.cardSurfaceSr.sortingOrder -= 500;
            baseAreaSlot.categoryCard.cardCanvas.sortingOrder -= 500;
        }
        baseAreaSlot.categoryData = category;
        
        // 隐藏计数文本
        if (baseAreaSlot.allCards.Count > 1)
        {
            baseAreaSlot.categoryCard.countText.enabled = false;
        }
        
        // 创建序列动画
        Sequence sequence = DOTween.Sequence();

        foreach (Card card in cards)
        {
            // 先与父物体卡组解绑
            card.transform.parent = null;
            
            // 跳过分类卡
            if (card.cardType == CardType.Category && baseAreaSlot.categoryCard.isTextScale) continue;
        
            // 获取目标位置
            Vector3 cardTarget = currentSlot.transform.position;
        
            // 移动卡牌位置
            sequence.Join(card.transform.DOMove(cardTarget, moveDuration))
                .SetEase(Ease.InOutQuad);
                    
            // 如果【放入卡组的卡牌数量大于1】且【当前遍历到的是最顶部卡牌】，则恢复文字大小
            if (cards.Count > 1 && card == cards[1])
            {
                card.MovingCardTextToBig();
            }
        }
    
        // 移动后的处理
        sequence.AppendCallback(() =>
        {
            if (baseAreaSlot.allCards.Count > 1)
            {
                if (baseAreaSlot.allCards.Count < baseAreaSlot.cardCount + 1)
                {
                    // 创建进度点
                    if (!baseAreaSlot.categoryCard.isTextScale)
                    {
                        baseAreaSlot.CreateProgressPoints();
                    }

                    // 点亮进度点
                    if (baseAreaSlot.progressPointSrs.Count > 0)
                    {
                        baseAreaSlot.LightUpProgressPoint();
                    }
                }
                else
                {
                    // 移除进度条
                    baseAreaSlot.RemoveProgressPoints();
                }
            }
            
            // 移动分类卡
            if (baseAreaSlot.allCards.Count > 1 && !baseAreaSlot.categoryCard.isTextScale)
            {
                baseAreaSlot.categoryCard.MovingCardTextToSmall();
                baseAreaSlot.categoryCard.MovingCardPos();
            }
            
            // 合并牌组
            if (baseAreaSlot.allCardGroups.Count > 0)
            {
                CardGroup group1 = this;
                CardGroup group2 = baseAreaSlot.allCardGroups.Pop();
        
                CardGroup newGroup = MergeGroup(group1, group2);
                baseAreaSlot.allCardGroups.Push(newGroup);
            }
            else
            {
                baseAreaSlot.allCardGroups.Push(this);
            }
        
            MovingCardGroupPosInBaseAreaSlot(baseAreaSlot); // 移动牌组位置

            if (baseAreaSlot.allCards.Count == baseAreaSlot.cardCount + 1)
            {
                baseAreaSlot.PackCards();
            }
        });
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
        canSelected = false;
    }
    
    #endregion
    
    #region 移动至取牌区相关
    
    /// <summary>
    /// 移动卡组至取牌区卡槽
    /// </summary>
    public void MovingCardToTakeAreaSlot(TakeAreaSlot takeAreaSlot)
    {
        cards[0].cardSurfaceSr.sortingOrder -= 999;
        if (cards[0].cardBorderSr != null)
        {
            cards[0].cardBorderSr.sortingOrder -= 999;
        }
        cards[0].cardCanvas.sortingOrder -= 999;
        
        takeAreaSlot.AddCard(this);
        
        isMoving = false;
    }
    
    #endregion
    
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
            });
    }
    
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
        
        // 获取附近所有牌组
        RaycastHit2D[] raycastHitsCardGroups = Physics2D.CircleCastAll(cards[0].transform.position, rayCastRadius,
            Vector2.down, rayCastDistance * (cards.Count - 1), LayerMask.GetMask("CardGroup"));
        
        Slot nearestSlot = null;
        float nearestDistance = float.MaxValue;
    
        // 寻找最近的卡槽
        foreach (RaycastHit2D slot in raycastHitsSlots)
        {
            // 忽略被占据的卡槽
            if (slot.collider.gameObject.GetComponent<Slot>().isOccupied) continue;
            
            // 计算距离
            float currentDistance = Vector3.Distance(slot.collider.transform.position, transform.position);
            
            // 更新最近距离
            if (currentDistance < nearestDistance)
            {
                nearestSlot = slot.collider.GetComponent<Slot>();
                nearestDistance = currentDistance;
            }
        }
        
        // 寻找最近的牌组
        foreach (RaycastHit2D cardGroup in raycastHitsCardGroups)
        {
            // 排除自己的卡牌
            if (cardGroup.collider.GetComponent<CardGroup>() == this) continue;
            
            // 排除不同分类的卡牌
            if (cardGroup.collider.GetComponent<CardGroup>().category != category) continue;
            
            // 排除取牌区的卡牌
            if (cardGroup.collider.GetComponent<CardGroup>().currentSlot != null && 
                cardGroup.collider.GetComponent<CardGroup>().currentSlot.slotType == SlotType.HomeSlot) continue;
            
            // 计算距离
            float currentDistance = Vector3.Distance(cardGroup.collider.transform.position, transform.position);
            
            // 更新最近距离
            if (currentDistance < nearestDistance)
            {
                nearestSlot = cardGroup.collider.GetComponent<CardGroup>().currentSlot;
                nearestDistance = currentDistance;
            }
        }

        // 关闭上一个高亮
        if (currentSelectSlot != null && currentSelectSlot != nearestSlot)
        {
            currentSelectSlot.DisableHighLight();
        }
        
        currentSelectSlot = nearestSlot;

        // 调整高亮边框大小并显示
        if (currentSelectSlot != null && !currentSelectSlot.isLighting && 
            (currentSelectSlot.allCardGroups.Count == 0 || currentSelectSlot.allCardGroups.Peek().category == category))
        {
            // 叠牌区高亮
            if (currentSelectSlot as StackAreaSlot != null)
            {
                // 不对分类卡高亮
                if (currentSelectSlot.allCardGroups.Count != 0 &&
                    currentSelectSlot.allCards.Peek().cardType == CardType.Category) return;
                
                float gap = currentSelectSlot.allCardGroups.Count == 0 ? 0 : (currentSelectSlot.allCardGroups.Peek().cards.Count - 1) * (currentSelectSlot as StackAreaSlot).gapSize;
                float pos = currentSelectSlot.allCardGroups.Count == 0
                    ? currentSelectSlot.transform.position.y
                    : currentSelectSlot.allCardGroups.Peek().transform.position.y;
                
                currentSelectSlot.borderRenderer.size = new Vector2(
                        currentSelectSlot.borderRenderer.size.x, 
                        currentSelectSlot.originHeight + gap * 1.9f
                    );

                currentSelectSlot.borderRenderer.gameObject.transform.position = new Vector3(
                        currentSelectSlot.borderRenderer.transform.position.x,
                        pos,
                        currentSelectSlot.borderRenderer.transform.position.z
                    );
                
                currentSelectSlot.EnableHighLight();
            }

            // 基础区高亮
            if (currentSelectSlot as BaseAreaSlot != null && 
                ((currentSelectSlot.allCards.Count == 0 && cards[0].cardType == CardType.Category) ||
                (currentSelectSlot.allCards.Count != 0 && cards[0].cardCategory == (currentSelectSlot as BaseAreaSlot).categoryCard.cardCategory)))
            {
                float gap = currentSelectSlot.allCards.Count < 2 ? 0 : (currentSelectSlot as BaseAreaSlot).categoryCard.transform.position.y -
                            currentSelectSlot.transform.position.y;
                
                currentSelectSlot.borderRenderer.size = new Vector2(
                        currentSelectSlot.borderRenderer.size.x,
                        currentSelectSlot.originHeight + gap * 1.9f
                    );

                currentSelectSlot.borderRenderer.gameObject.transform.position = new Vector3(
                    currentSelectSlot.borderRenderer.transform.position.x,
                    currentSelectSlot.transform.position.y + gap / 2,
                        currentSelectSlot.borderRenderer.transform.position.z
                    );
                
                currentSelectSlot.EnableHighLight();
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
    
    #region 游戏开始时的逻辑
    
    /// <summary>
    /// 在开始时朝叠牌区发牌
    /// </summary>
    public Vector3 DealCardToStackSlotOnStart(StackAreaSlot stackAreaSlot)
    {
        // 设置卡槽为被占据状态
        stackAreaSlot.isOccupied = true;
        currentSlot = stackAreaSlot;
            
        // 将卡牌添加到卡槽中
        stackAreaSlot.allCards.Push(cards[0]);
        
        // 将牌组添加到卡槽中
        stackAreaSlot.AddCard(this);

        return stackAreaSlot.transform.position + new Vector3(0, -(stackAreaSlot.allCards.Count - 1) * stackAreaSlot.gapSize, -(stackAreaSlot.allCards.Count));
    }
    
    /// <summary>
    /// 在开始时朝叠牌区发牌
    /// </summary>
    public Vector3 DealCardToBaseSlotOnStart(BaseAreaSlot baseAreaSlot)
    {
        cards[0].FlipCard(0);
        
        // 设置卡槽为被占据状态
        baseAreaSlot.isOccupied = true;
        currentSlot = baseAreaSlot;
            
        // 将卡牌添加到卡槽中
        baseAreaSlot.allCards.Push(cards[0]);
        
        // 将牌组添加到卡槽中
        baseAreaSlot.AddCard(this);
        
        // 设置分类卡
        if (cards[0].cardType == CardType.Category)
        {
            baseAreaSlot.SetCategoryCard(cards[0] as CategoryCard);
        }

        return baseAreaSlot.transform.position;
    }
    
    #endregion
    
    #region 红点计数

    public void DisplayCount()
    {
        int count = 0;

        foreach (var card in cards)
        {
            if (card.cardType == CardType.Character)
            {
                count++;
            }
        }
        
        if (count <= 1) return;
        
        redPoint.SetActive(true);
        
        // 更新位置
        redPoint.transform.position = new Vector3(transform.position.x, transform.position.y + redPointUpDistance * (cards.Count - 1), transform.position.z);

        // 更新文本
        countText.text = count.ToString();
    }
    
    #endregion
}
