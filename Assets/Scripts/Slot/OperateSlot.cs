using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class OperateSlot : Slot {
    public float gapSize; // 两牌之间的间距
    public float moveDuration; // 移动到卡槽的时间
    public float maxCardCount; // 压缩前最多可放置的牌数
    public float compressionCoefficient; // 压缩系数

    /// <summary>
    /// 更新卡牌位置
    /// </summary>
    public override Sequence UpdateCardsPos() {
        // 创建动画序列
        Sequence sequence = DOTween.Sequence();

        int count = cards.Count - 1; // 计数器
        
        List<CardActor> cardList = cards.ToList();

        for (int i = count; i >= 0; i--)
        {
            CardActor card = cardList[i];
            Vector3 targetPos; // 目标位置
            
            card.isMoving = true;

            // 让最后一张牌的位置与卡槽位置重合
            if (i == count)
            {
                targetPos = transform.position;
            }
            else
            {
                float newGapSize = -gapSize;

                // 如果超过了最大牌数
                if (cards.Count > maxCardCount)
                    newGapSize = -gapSize / 1.5f / ((cards.Count - maxCardCount) * compressionCoefficient + 1);
                else
                    newGapSize = -gapSize / 1.5f;
                
                // 如果是盖牌，则目标位置为上一张牌的位置减去计算后的newGapSize
                if (cardList[i + 1].isFaceDown)
                {
                    targetPos = cardList[i + 1].targetPos + new Vector3(0, newGapSize, 0);
                }
                // 如果是明牌，则目标位置为上一张牌位置减去gapSize
                else
                {
                    targetPos = cardList[i + 1].targetPos + new Vector3(0, -gapSize, 0);
                }
            }
            
            card.targetPos = targetPos;
        }

        SetCardsPos(sequence);
        UpdateKeyPosAndHeight();

        return sequence;
    }

    /// <summary>
    /// 设置每张牌到新的位置
    /// </summary>
    public void SetCardsPos(Sequence sequence)
    {
        foreach (CardActor card in cards)
        {
            // 创建tween，为每一张牌单独设置OnComplete
            Tweener tween = card.transform.DOMove(card.targetPos, moveDuration);
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
        }
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
            
            float newGapSize = -gapSize;
            
            // 如果超过了最大牌数
            if (cards.Count > maxCardCount)
                newGapSize = -gapSize / 1.5f / ((cards.Count - maxCardCount) * compressionCoefficient + 1);
            else
                newGapSize = -gapSize / 1.5f;

            // 计算keyPos偏移量，遇到盖牌向下移动计算后的gapSize，遇到明牌则移动半个gapSize
            posOffset += card.isFaceDown ? -newGapSize : gapSize / 2;
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
