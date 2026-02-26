using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterGroup : MonoBehaviour
{
    public Vector3 originPos;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        originPos = rectTransform.anchoredPosition;
    }
    
    public void SetPos()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = originPos;
        }
    }
}
