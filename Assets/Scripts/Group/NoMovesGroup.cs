using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class NoMovesGroup : MonoBehaviour
{
    public Image mask;
    
    public void ShowPanel(bool anim = true)
    {
        gameObject.SetActive(true);
        gameObject.transform.DOMoveY(0, anim ? 0.2f : 0f);
        mask.color = new Color(mask.color.r, mask.color.g, mask.color.b, 0);
        mask.DOFade(240f / 255f, 0.2f);
        
        int year = GameDataUtils.Instance.levelId / 10000;
        int month = GameDataUtils.Instance.levelId % 1000 / 100;
        int day = GameDataUtils.Instance.levelId % 100;
        GameDataUtils.Instance.solitaireScene.calenderGroup.Init(year, month, day);

        GameDataUtils.Instance.isGaming = false;
        GameStateManager.Instance.ClearSavedGameState();
    }
    
    public void ClosePanel(bool anim = true)
    {
        gameObject.transform.DOMoveY(1500, anim ? 0.2f : 0f)
            .OnComplete(() => {
                gameObject.SetActive(false);
            });
        
        mask.color = new Color(mask.color.r, mask.color.g, mask.color.b, 240f / 255f);
        mask.DOFade(0, 0.2f);
        GameDataUtils.Instance.solitaireScene.SetModeAndPlay(4);
    }
}
