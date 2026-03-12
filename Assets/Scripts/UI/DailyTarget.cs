using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyTarget : MonoBehaviour
{
    public Text progressText; // 进度文本

    /// <summary>
    /// 初始化进度条
    /// </summary>
    public void Init(int aimCount) {
        if (aimCount <= 0)
            aimCount = 1;
        
        GameDataUtils.Instance.completeCount = 0;
        progressText.text = $"0/{aimCount}";
    }

    /// <summary>
    /// 设置进度条
    /// </summary>
    public void SetTarget(int completeCount, int aimCount, bool anim = true) {
        progressText.text = $"{completeCount}/{aimCount}";
    }
}
