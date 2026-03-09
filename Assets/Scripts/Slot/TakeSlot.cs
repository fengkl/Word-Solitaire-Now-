using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TakeSlot : Slot {
    [Header("卡牌信息")]
    public float frontGapSize; // 有字卡牌间隔
    public float backGapSize; // 折叠卡牌间隔
    public float maxFrontDisplayCount = 3; // 最大有字卡牌显示数量
    public float maxBackDisplayCount = 3; // 最大折叠卡牌显示数量

    [Header("动画参数")]
    public float moveToTakeAreaTime; // 移动时间

    /// <summary>
    /// 更新卡牌位置
    /// </summary>
    public override Sequence UpdateCardsPos() {
        // 创建动画序列
        Sequence sequence = DOTween.Sequence();
        Tween tween;

        int count = 0;

        foreach (var card in cards) {
            card.transform.SetAsFirstSibling();

            // 如果取牌区没牌
            if (count == 0) {
                tween = card.transform.DOMove(new Vector3(transform.position.x, transform.position.y, transform.position.z + 1), moveToTakeAreaTime);
                sequence.Join(tween);
                card.MovingCardTextToBig();
                count++;
            }
            // 如果取牌区有牌，且还没达到折叠文字的数量
            else if (count < maxFrontDisplayCount) {
                tween = card.transform.DOMove(new Vector3(transform.position.x,
                                              transform.position.y + frontGapSize * count,
                                              count + 1),
                                              moveToTakeAreaTime);
                sequence.Join(tween);
                card.MovingCardTextToSmall();
                count++;
            }
            // 如果取牌区有牌，且已经达到折叠文字的数量
            else if (count < maxFrontDisplayCount + maxBackDisplayCount) {
                tween = card.transform.DOMove(new Vector3(transform.position.x,
                                              transform.position.y + frontGapSize * (maxFrontDisplayCount - 1) + backGapSize * (count - maxFrontDisplayCount + 1),
                                              count + 1),
                                              moveToTakeAreaTime);
                sequence.Join(tween);
                count++;
            }
        }

        UpdateCardsCanSelect();

        return sequence;
    }

    /// <summary>
    /// 设置卡牌是否可以拿取
    /// </summary>
    private void UpdateCardsCanSelect() {
        foreach (var card in cards)
            card.canSelected = card == cards.Peek();
    }
}
