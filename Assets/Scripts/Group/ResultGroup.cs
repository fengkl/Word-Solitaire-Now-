using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ResultGroup : MonoBehaviour {
    public Text timer;
    public Text mode;
    public Text score;

    public void Init() {
        timer.text = GameDataUtils.Instance.solitaireScene.topGroup.gameTimer.timerText.text;
        mode.text = GameDataUtils.Instance.mode.ToString();
        score.text = GameDataUtils.Instance.cardActors.Count.ToString();
    }

    public void Continue() {
        GameDataUtils.Instance.solitaireScene.selectLevelPanelGroup.ShowSelectLevelPanel();
    }

    public void BackToHome() {
        GameDataUtils.Instance.levelId = 0;
        GameDataUtils.Instance.solitaireScene.BackToMenu();
        gameObject.SetActive(false);
    }
}
