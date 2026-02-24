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
    
    public ModePanel modePanel; // 模式选择面板
    private Vector3 originModePanelPos; // 模式选择面板位置

    public bool isMovingPanel; // 是否正在移动面板

    /// <summary>
    ///  显示难度选择面板
    /// </summary>
    public void ShowModePanel()
    {
        if (isGameStart || isMovingPanel) return;
        
        isMovingPanel = true;
        
        originModePanelPos = modePanel.transform.position;
        
        // 创建动画序列
        Sequence sequence = DOTween.Sequence();
        
        sequence.Join(modePanel.transform.DOMove(Vector3.zero, 0.5f)
            .SetEase(Ease.InOutQuad));
        sequence.Join(modePanel.bg.DOFade(0.8f, 0.5f));
        sequence.AppendCallback(() => { isMovingPanel = false; });
    }

    /// <summary>
    /// 关闭难度选择面板
    /// </summary>
    public void CloseModePanel()
    {
        if (isMovingPanel) return;
        
        isMovingPanel = true;
        
        // 创建动画序列
        Sequence sequence = DOTween.Sequence();
        
        sequence.Join(modePanel.transform.DOMove(originModePanelPos, 0.5f)
            .SetEase(Ease.OutQuad));
        sequence.Join(modePanel.bg.DOFade(0, 0.5f));
        sequence.AppendCallback(() => { isMovingPanel = false; });
    }
    
    /// <summary>
    /// 设置难度
    /// </summary>
    public void SetModeAndPlay(int modeIndex)
    {
        modePanel.SetMode(modeIndex);
        StartGame();
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    private void StartGame()
    {
        if (isGameStart) return;
        
        isGameStart = true;
        
        // 创建动画序列
        Sequence sequence = DOTween.Sequence();
        
        
        foreach (var gameImage in GameImages)
        {
            sequence.Join(gameImage.DOFade(1, fadeTime));
            gameImage.raycastTarget = true;
        }
        
        foreach (var gameText in GameTexts)
        {
            sequence.Join(gameText.DOFade(1, fadeTime));
        }

        sequence.AppendCallback(() =>
        {
            GameManager.Instance.InitGame();
            gameObject.SetActive(false);
        });

        sequence.AppendCallback(CloseModePanel);
    }
}
