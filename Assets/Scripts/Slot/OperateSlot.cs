using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class OperateSlot : Slot {
    public float gapSize; // 两牌之间的间距
    public float moveDuration; // 移动到卡槽的时间

    /// <summary>
    /// 更新卡牌位置
    /// </summary>
    public override Sequence UpdateCardsPos() {
        // 创建动画序列
        Sequence sequence = DOTween.Sequence();

        int count = cards.Count - 1; // 计数器

        // 遍历卡组中的所有卡牌,设置位置
        foreach (CardActor card in cards) {
            card.isMoving = true;

            // 获取目标位置
            Vector3 targetPos = transform.position + new Vector3(0, -count * gapSize, 0);

            // 创建tween，为每一张牌单独设置OnComplete
            Tweener tween = card.transform.DOMove(targetPos, moveDuration);
            tween.SetEase(Ease.InOutQuad);
            tween.OnComplete(() => {
                card.isMoving = false;
                card.transform.SetAsFirstSibling();
            });

            // 将 tween 加入 sequence
            sequence.Join(tween);

            // 如果当前卡牌不是【盖着】【不是栈顶卡牌】且【文字未缩放过】
            if (!card.isFaceDown && card != cards.Peek() && !card.isTextScale)
                card.MovingCardTextToSmall();

            count--;
        }

        UpdateKeyPosAndHeight();

        return sequence;
    }

    /// <summary>
    /// 更新keyPos和slotHeight
    /// </summary>
    public override void UpdateKeyPosAndHeight() {
        float posOffset = 0;
        float height = (transform as RectTransform).rect.height + 4;

        bool isFirstCard = true;

        // 遍历卡组中的所有卡牌，计算偏移
        foreach (CardActor card in cards) {
            if (isFirstCard) {
                isFirstCard = false;
                continue;
            }

            // 计算keyPos偏移量，遇到盖牌向下移动满gapSize，遇到明牌则移动半个gapSize
            posOffset += card.isFaceDown ? gapSize : gapSize / 2;
            // 计算高度，遇到盖牌不变，遇到明牌则增加gapSize的数值
            height += card.isFaceDown ? 0 : gapSize;
        }

        keyPos = new Vector3(transform.position.x, transform.position.y - posOffset, 0);
        slotHeight = height;
    }

    /// <summary>
    /// 检查是否有可翻转的卡牌
    /// </summary>
    public void CheckFlipCard() {
        if (cards.Count > 0 && cards.Peek().isFaceDown)
            cards.Peek().FlipCard();
    }
}
