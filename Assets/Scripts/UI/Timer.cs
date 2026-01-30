using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText; //计时器文本
    public float time; // 经过时间
    private float minutes; // 分钟
    private float seconds; // 秒
    public bool isStart; // 是否开始计时

    void Update()
    {
        UpdateTimerText();
    }

    /// <summary>
    /// 改变计时器文本
    /// </summary>
    private void UpdateTimerText()
    {
        if (!isStart) return;
        
        time += Time.deltaTime;
        minutes = Mathf.FloorToInt(time / 60f);
        seconds = Mathf.FloorToInt(time % 60f);
        timerText.text = minutes >= 100 ? $"{minutes:D3}:{seconds:00}" : $"{minutes:00}:{seconds:00}";
    }
}
