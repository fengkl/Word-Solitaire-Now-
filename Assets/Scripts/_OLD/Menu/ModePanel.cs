using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModePanel : MonoBehaviour
{
    public Image bg;

    public Text easyLevelText;
    public Text MediumLevelText;
    public Text HardLevelText;

    private void Start()
    {
        easyLevelText.text = "Level " + LevelData.currentEasyLevelId;
        MediumLevelText.text = "Level " + LevelData.currentMediumLevelId;
        HardLevelText.text = "Level " + LevelData.currentHardLevelId;
    }

    /// <summary>
    /// …Ë÷√ƒ—∂»
    /// </summary>
    public void SetMode(int modeIndex)
    {
        LevelMode mode = (LevelMode)modeIndex;
        GameLevelManager.Instance.LoadLevel(mode);
    }
}
