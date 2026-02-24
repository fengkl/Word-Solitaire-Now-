using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanel : MonoBehaviour
{
    public Text timeText;
    public Text modeText;
    public Text scoreText;
    private LevelMode mode;

    public GameObject MenuScene;
    public List<Image> GameImages; // 游戏界面ui
    public List<TextMeshProUGUI> GameTexts; // 游戏界面文字
    public float fadeTime; // 淡入淡出时间

    /// <summary>
    ///  设置结果
    /// </summary>
    public void SetResult(float time, int mode, int score)
    {
        float minutes; // 分钟
        float seconds; // 秒
        
        minutes = Mathf.FloorToInt(time / 60f);
        seconds = Mathf.FloorToInt(time % 60f);
        timeText.text = minutes >= 100 ? $"{minutes:D3}:{seconds:00}" : $"{minutes:00}:{seconds:00}";
        
        modeText.text = mode.ToString();
        scoreText.text = score.ToString();
        
        this.mode = (LevelMode)mode;
    }

    /// <summary>
    /// 下一关
    /// </summary>
    public void NextLevel()
    {
        GameManager.Instance.ClearGameBoard();
        GameLevelManager.Instance.LoadLevel(mode);
        
        GameManager.Instance.InitGame();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void BackToMenu()
    {
        GameManager.Instance.ClearGameBoard();
        
        MenuScene.SetActive(true);
        
        // 创建动画序列
        Sequence sequence = DOTween.Sequence();
        
        
        foreach (var gameImage in GameImages)
        {
            sequence.Join(gameImage.DOFade(1, fadeTime));
            gameImage.raycastTarget = false;
        }
        
        foreach (var gameText in GameTexts)
        {
            sequence.Join(gameText.DOFade(1, fadeTime));
        }

        sequence.AppendCallback(() =>
        {
        });
    }
}
