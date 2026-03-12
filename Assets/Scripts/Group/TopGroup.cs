using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopGroup : MonoBehaviour {
    public GameTimer gameTimer;
    public GameProgressBar gameProgressBar;
    public GameMode gameMode;
    public DailyMoves dailyMoves;
    public DailyTarget dailyTarget;

    /// <summary>
    /// 初始化布局
    /// </summary>
    public void Init(LevelMode mode)
    {
        if (mode == LevelMode.Daily)
        {
            gameMode.gameObject.SetActive(true);
            gameProgressBar.gameObject.SetActive(false);
            gameTimer.gameObject.SetActive(false);
            dailyMoves.gameObject.SetActive(true);
            dailyTarget.gameObject.SetActive(true);
        }
        else
        {
            gameMode.gameObject.SetActive(true);
            gameProgressBar.gameObject.SetActive(true);
            gameTimer.gameObject.SetActive(true);
            dailyMoves.gameObject.SetActive(false);
            dailyTarget.gameObject.SetActive(false);
        }
    }
}
