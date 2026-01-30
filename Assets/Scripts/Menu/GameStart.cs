using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameStart : MonoBehaviour
{
    public List<Image> MenuImages; // 主界面ui
    public List<Text> MenuTexts; // 主界面文字
    public List<Image> GameImages; // 游戏界面ui
    public List<TextMeshProUGUI> GameTexts; // 游戏界面文字
    public float fadeTime; // 淡入淡出时间
    public bool isGameStart; // 游戏是否开始

    /// <summary>
    /// 开始游戏
    /// </summary>
    public void StartGame()
    {
        if (isGameStart) return;
        
        isGameStart = true;
        
        // 创建动画序列
        Sequence sequence = DOTween.Sequence();

        foreach (var menuUIImage in MenuImages)
        {
            sequence.Join(menuUIImage.DOFade(0, fadeTime));
        }
        
        foreach (var menuText in MenuTexts)
        {
            sequence.Join(menuText.DOFade(0, fadeTime));
        }

        sequence.AppendCallback(() =>
        {
            GameManager.Instance.InitGame();
        });
        
        foreach (var gameImage in GameImages)
        {
            sequence.Join(gameImage.DOFade(1, fadeTime));
        }
        
        foreach (var gameText in GameTexts)
        {
            sequence.Join(gameText.DOFade(1, fadeTime));
        }
    }
}
