using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SelectLevelPanelGroup : MonoBehaviour {
    public Image mask;
    
    public Text easyLevelText;
    public Text MediumLevelText;
    public Text HardLevelText;

    /// <summary>
    /// 更新关卡数文本
    /// </summary>
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
        mask.DOFade(240f / 255f, 0.2f);
    }

    /// <summary>
    /// 关闭选关面板
    /// </summary>
    public void CloseSelectLevelPanel(bool anim = true) {
        gameObject.transform.DOMoveY(-1500, anim ? 0.2f : 0).OnComplete(() => {
            gameObject.SetActive(false);
        });
        
        mask.DOFade(0, 0.2f);
    }
}
