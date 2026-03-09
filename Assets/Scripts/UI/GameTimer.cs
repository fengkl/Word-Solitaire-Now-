using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour {
    public Text timerText; //计时器文本
    public float time; // 经过时间
    private float minutes; // 分钟
    private float seconds; // 秒
    public bool isStart; // 是否开始计时

    //void Update() {
    //    UpdateTimerText();
    //}

    public void StartTimer() {
        isStart = true;
    }

    public void StopTimer() {
        isStart = false;
    }

    public void ResetTimer() {
        //time = 0f;
        timerText.text = "0";
    }

    /// <summary>
    /// 改变计时器文本
    /// </summary>
    public void UpdateMove() {
        //if (!isStart) return;

        //time += Time.deltaTime;
        //minutes = Mathf.FloorToInt(time / 60f);
        //seconds = Mathf.FloorToInt(time % 60f);
        //timerText.text = minutes >= 100 ? $"{minutes:D3}:{seconds:00}" : $"{minutes:00}:{seconds:00}";

        timerText.text = "" + GameDataUtils.Instance.moveCount ;
    }
}
