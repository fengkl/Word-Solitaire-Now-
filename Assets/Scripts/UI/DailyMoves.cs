using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyMoves : MonoBehaviour
{
    public Text MovesText; // 进度文本
    
    public void InitMovesText(int moves)
    {
        MovesText.text = $"{moves}";
        GameDataUtils.Instance.leftMoveCount = moves;
        MovesText.color = Color.white;
    }

    public void UpdateMovesText()
    {
        bool isComplete = true;

        foreach (var stack in GameDataUtils.Instance.operateSlots)
        {
            if (stack.cards.Count != 0)
                isComplete = false;
        }
        
        foreach (var stack in GameDataUtils.Instance.takeSlots)
        {
            if (stack.cards.Count != 0)
                isComplete = false;
        }
        
        if (--GameDataUtils.Instance.leftMoveCount <= 0)
            GameDataUtils.Instance.leftMoveCount = 0;
        
        if (GameDataUtils.Instance.leftMoveCount <= 0 && !isComplete)
            GameDataUtils.Instance.solitaireScene.noMovesGroup.ShowPanel();
        
        MovesText.color = GameDataUtils.Instance.leftMoveCount <= 5 ? Color.red : Color.white;
        
        MovesText.text = $"{GameDataUtils.Instance.leftMoveCount}";
    }
}
