using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SelectLevelPanelGroup : MonoBehaviour {
    public Text easyLevelText;
    public Text MediumLevelText;
    public Text HardLevelText;

    public void UpdateText() {
        easyLevelText.text = "Level " + LevelData.levelIdDict[LevelMode.Easy];
        MediumLevelText.text = "Level " + LevelData.levelIdDict[LevelMode.Normal];
        HardLevelText.text = "Level " + LevelData.levelIdDict[LevelMode.Hard];
    }

    /// <summary>
    /// 显示选关面板
    /// </summary>
    public void ShowSelectLevelPanel() {
        UpdateText();
        gameObject.SetActive(true);
        gameObject.transform.DOMoveY(-350, 0.2f);
    }

    /// <summary>
    /// 关闭选关面板
    /// </summary>
    public void CloseSelectLevelPanel(int customDuration = -1) {
        gameObject.transform.DOMoveY(-1500, customDuration != -1 ? customDuration : 0.2f).OnComplete(() => {
            gameObject.SetActive(false);
        });
    }
}
