using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopGroup : MonoBehaviour
{
    public GameTimer gameTimer;
    public GameProgressBar gameProgressBar;
    public GameMode gameMode; 

    public void StartTimer()
    {
        gameTimer.StartTimer();
    }

    public void InitProgressBar(int aimCOunt)
    {
        gameProgressBar.Init(aimCOunt);
    }
}
