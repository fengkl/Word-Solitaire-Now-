using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CounterGroup : MonoBehaviour
{
    public Text counterText;
    
    public Vector2 originPos;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        originPos = rectTransform.anchoredPosition;
    }

    /// <summary>
    /// 禁用计数器
    /// </summary>
    public void DisableCounter()
    {
        counterText.text = "0";
        counterText.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 重置到原始位置
    /// </summary>
    public void SetPos()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = originPos;
        }
    }
    
    /// <summary>
    /// 设置位置
    /// </summary>
    public void SetPos(Vector2 pos)
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = pos;
            print(rectTransform.anchoredPosition);
        }
    }
}
