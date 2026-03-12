using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HomeSlot : MonoBehaviour, IPointerClickHandler {
    [Header("基本信息")]
    public Stack<CardActor> cards = new Stack<CardActor>(); // 卡牌栈
    public float gapSize; // 卡牌间隔
    public int maxDisplayCount; // 最大显示数量
    public TakeSlot leftTakeSlot; // 左侧取牌区域
    public TakeSlot rightTakeSlot; // 右侧取牌区域

    [Header("初始化动画参数")]
    public float initMoveTime; // 移动时间

    [Header("翻牌动画参数")]
    public float flipCD; // 翻牌冷却时间
    public float recycleCD; // 回收冷却时间
    public bool isMovingCard; // 是否正在移动牌
    public bool isRefreshing; // 是否正在刷新

    [Header("剩余数量显示")]
    public GameObject counter; // 剩余数量显示框
    public Text countText; // 剩余数量文本

    [Header("测试用")]
    public List<CardActor> testCardGroups = new List<CardActor>();

    private void Update() {
        testCardGroups = new List<CardActor>(cards);
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (isMovingCard || isRefreshing) return;
        
        // 更新剩余步数计数器
        GameDataUtils.Instance.solitaireScene.topGroup.dailyMoves.UpdateMovesText();

        // 如果homeSlot中还有牌，那么移动到取牌区，否则将取牌区中的所有牌回收
        GameDataUtils.Instance.moveCount++;
        GameDataUtils.Instance.solitaireScene.topGroup.gameTimer.UpdateMove();
        
        if (cards.Count > 0) {
            isMovingCard = true;

            FlipCardAndMovingToTakeSlot();

            Invoke("ChangeIsMovingCard", flipCD);
        } else {
            Stack<CardActor> leftSlot = leftTakeSlot.cards;
            Stack<CardActor> rightSlot = rightTakeSlot.cards;

            isRefreshing = true;

            Sequence sequence = DOTween.Sequence();

            // 聚牌
            GatherCards(sequence, leftTakeSlot);
            GatherCards(sequence, rightTakeSlot);

            sequence.OnComplete(() => {
                // 先将多的部分取出来
                if (leftSlot.Count > rightSlot.Count)
                    AddCard(leftSlot.Pop());
                
                // 合并卡组
                while (leftSlot.Count > 0 || rightSlot.Count > 0) {
                    if (leftSlot.Count > 0)
                        AddCard(leftSlot.Pop());

                    if (rightSlot.Count > 0)
                        AddCard(rightSlot.Pop());
                }

                // 回收
                RecycleCards();
            });

            Invoke("ChangeIsRefreshing", recycleCD);
        }
    }

    /// <summary>
    /// 添加卡牌
    /// </summary>
    public void AddCard(CardActor card) {
        // 入栈
        cards.Push(card);

        card.transform.SetAsLastSibling();
    }

    /// <summary>
    /// 更新卡牌位置
    /// </summary>
    public void UpdateCardsPos() {
        int count = 1;

        // 设置每张卡牌的位置
        foreach (CardActor card in cards) {
            Vector3 target = new Vector3(transform.position.x - (count - 1) * gapSize, transform.position.y, count);

            card.transform.DOMove(target, initMoveTime)
            .SetEase(Ease.InOutQuad);

            // 限制显示的最大数量
            if (count < maxDisplayCount)
                count++;
        }

        count = cards.Count < maxDisplayCount ? cards.Count : maxDisplayCount;

        // 更新计数器位置
        Vector3 DisplayCountPosTarget = new Vector3(
            transform.position.x - 67 - (count - 1) * gapSize,
            counter.transform.position.y,
            count
        );

        counter.transform.DOMove(DisplayCountPosTarget, initMoveTime)
        .SetEase(Ease.InOutQuad);

        // 显示计数器
        if (count != 0)
            DisplayCount();

        // 更新剩余数量
        UpdateCount();
    }

    /// <summary>
    /// 翻牌并移动到取牌区域
    /// </summary>
    private void FlipCardAndMovingToTakeSlot() {
        CardActor card1;
        CardActor card2;

        // 如果homeSlot余牌两张及以上，则取两张，否则取一张
        if (cards.Count > 1) {
            // 先后取两次
            card1 = cards.Pop();
            card2 = cards.Pop();

            // 分别按顺序向右和左添加牌组
            rightTakeSlot.AddCard(card1);
            leftTakeSlot.AddCard(card2);

            // 翻牌
            card1.FlipCard();
            card2.FlipCard();
        } else {
            // 只取一次，且添加到左侧
            card1 = cards.Pop();
            leftTakeSlot.AddCard(card1);

            // 翻牌
            card1.FlipCard();
        }

        // 更新剩余牌位置
        UpdateCardsPos();
    }

    /// <summary>
    /// 聚牌
    /// </summary>
    private void GatherCards(Sequence sequence, TakeSlot slot) {
        // 移动牌组,将所有牌摞到一起
        foreach (var card in slot.cards) {
            sequence.Join(card.transform.DOMove(slot.transform.position, slot.moveToTakeAreaTime));
            card.MovingCardTextToBig();
        }
    }

    /// <summary>
    /// 回收卡牌
    /// </summary>
    private void RecycleCards() {
        // 创建序列动画
        Sequence sequence = DOTween.Sequence();

        // 移动牌组,将所有牌放回去
        foreach (var card in cards) {
            sequence.Join(card.transform.DOMove(transform.position, initMoveTime));
            card.ReverseFlipCard();
        }

        sequence.AppendCallback(UpdateCardsPos);
    }

    /// <summary>
    /// 翻牌冷却结束
    /// </summary>
    private void ChangeIsMovingCard() {
        isMovingCard = false;
    }

    /// <summary>
    /// 回收冷却结束
    /// </summary>
    private void ChangeIsRefreshing() {
        isRefreshing = false;
    }

    /// <summary>
    ///  显示数量
    /// </summary>
    public void DisplayCount() {
        counter.SetActive(true);
    }

    /// <summary>
    /// 更新剩余数量
    /// </summary>
    public void UpdateCount(int count = -1) {
        if (cards.Count > 0)
            countText.text = cards.Count.ToString();
        else {
            if (count != -1)
                countText.text = count.ToString();
            else
                counter.SetActive(false);
        }
    }
}
