using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move {
    public Slot from; // 出发的卡槽
    public Slot to; // 到达的卡槽
    public int cardCount = 0; // 移动的卡牌数量
    public string category; // 卡牌的类型
    public CategoryCardActor categoryCard; // 分类卡
    public Vector3 keyPos; // 卡牌的关键位置，用于计算到卡槽的距离

    public Stack<CardActor> cards = new Stack<CardActor>(); // 移动的卡牌列表

    public Move(Slot from, string category) {
        this.from = from;
        this.category = category;
    }

    public void MoveToMoveGroup() {
        foreach (var card in cards) {
            card.transform.SetParent(GameDataUtils.Instance.solitaireScene.moveGroup.transform);
            card.transform.SetAsLastSibling();
        }
    }

    public void MoveToCardGroup() {
        foreach (var card in cards) {
            card.transform.SetParent(GameDataUtils.Instance.solitaireScene.cardGroup.transform);
            // 特别处理目标卡槽中的分类卡，使其层级位于最下
            if (card.cardType == CardType.Category && card.currentSlot as TargetSlot != null)
                card.transform.SetAsFirstSibling();
            else
                card.transform.SetAsLastSibling();
        }
    }
}
