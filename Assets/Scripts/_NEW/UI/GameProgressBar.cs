using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameProgressBar : MonoBehaviour
{
    public GameObject progressBar; // 进度条
    public Text progressText; // 进度文本
    public float moveSpeed; // 进度条涨速

    /// <summary>
    /// 初始化进度条
    /// </summary>
    public void Init(int aimCount)
    {
        if (aimCount <= 0)
        {
            aimCount = 1;
        }
        
        progressBar.transform.localScale = new Vector3(0, progressBar.transform.localScale.y, progressBar.transform.localScale.z);
        progressText.text = $"0/{aimCount}";
    }
    
    /// <summary>
    /// 设置进度条
    /// </summary>
    public void SetProgress(int completeCount, int aimCount)
    {
        progressText.text = $"{completeCount}/{aimCount}";
        progressBar.transform.DOScaleX((float)completeCount/aimCount, moveSpeed);
    }
}
