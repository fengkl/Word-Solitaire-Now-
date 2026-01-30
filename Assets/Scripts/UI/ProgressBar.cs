using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ProgressBar : MonoBehaviour
{
    public GameObject progressBar; // 进度条
    public TextMeshProUGUI progressText; // 进度文本
    public float moveSpeed; // 进度条涨速

    /// <summary>
    /// 初始化进度条
    /// </summary>
    public void Init(int aimCount)
    {
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
