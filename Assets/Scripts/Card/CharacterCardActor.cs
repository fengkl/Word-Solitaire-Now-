using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCardActor : CardActor
{
    /// <summary>
    /// 初始化卡牌信息(文字卡)
    /// </summary>
    public void Init(CardType type, string category, string name)
    {
        cardType = type;
        cardCategory = category;
        cardName = name;
        nameText.text = cardName.Split("#")[0];
        isFaceDown = true;
        originScale = transform.localScale.x;
    }
}
