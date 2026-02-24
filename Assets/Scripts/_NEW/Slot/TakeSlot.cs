using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TakeSlot : Slot_New
{
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
    public override Sequence UpdateCardsPos()
    {
        int count = 0;

        foreach (var card in cards)
        {
            // 如果取牌区没牌
            if (count == 0)
            {
                card.transform.DOMove(new Vector3(transform.position.x, transform.position.y, transform.position.z + 1), moveToTakeAreaTime);
                card.MovingCardTextToBig();
                count++;
            }
            // 如果取牌区有牌，且还没达到折叠文字的数量
            else if (count < maxFrontDisplayCount)
            {
                card.transform.DOMove(new Vector3(transform.position.x, 
                        transform.position.y + frontGapSize * count, 
                        count + 1), 
                    moveToTakeAreaTime);
                card.MovingCardTextToSmall();
                count++;
            }
            // 如果取牌区有牌，且已经达到折叠文字的数量
            else if (count < maxFrontDisplayCount + maxBackDisplayCount)
            {
                card.transform.DOMove(new Vector3(transform.position.x, 
                        transform.position.y + frontGapSize * (maxFrontDisplayCount - 1) + backGapSize * (count - maxFrontDisplayCount + 1), 
                        count + 1), 
                    moveToTakeAreaTime);
                count++;
            }
        }

        return null;
    }
}
