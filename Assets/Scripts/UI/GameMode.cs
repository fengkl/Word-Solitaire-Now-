using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMode : MonoBehaviour {
    //public Text modeText;
    public Text levelText;

    public void SetModeText(LevelMode mode, int level = 1) {
        //modeText.text = mode;
        if (mode == LevelMode.Daily)
        {
            int year = GameDataUtils.Instance.levelId / 10000;
            int month = GameDataUtils.Instance.levelId % 1000 / 100;
            int day = GameDataUtils.Instance.levelId % 100;

            if (DateTime.Now.Year == year && DateTime.Now.Month == month && DateTime.Now.Day == day)
            {
                levelText.text = "Today";
            }
            else
            {
                levelText.text = year + "." + month + "." + day;
            }
        }
        else
        {
            levelText.text = "Level " + level;
        }
    }
}
