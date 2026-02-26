using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MenuGroup : MonoBehaviour
{
    public Image gameBg;
    public Image bg;
    public Image mask;
    public GameObject continueButton;
    public GameObject selectPanel;
    
    public Text continueGameLevelText;
    public Text easyLevelText;
    public Text MediumLevelText;
    public Text HardLevelText;

    private void Start()
    {
        SetContinueButton();
        UpdateText();
    }

    /// <summary>
    /// 更新文本
    /// </summary>
    public void UpdateText()
    {
        easyLevelText.text = "Level " + LevelData.levelIdDict[LevelMode.Easy];
        MediumLevelText.text = "Level " + LevelData.levelIdDict[LevelMode.Normal];
        HardLevelText.text = "Level " + LevelData.levelIdDict[LevelMode.Hard];
        continueGameLevelText.text = GameDataUtils.Instance.mode + "  Level " + GameDataUtils.Instance.levelId;
    }

    /// <summary>
    /// 设置继续游戏的按钮的显示
    /// </summary>
    public void SetContinueButton()
    {
        UpdateText();
        
        // 检查是否有保存的残局
        if (GameStateManager.Instance.HasSaveGameState())
        {
            continueButton.SetActive(true);
        }
        else
        {
            continueButton.SetActive(false);
        }
    }

    /// <summary>
    /// 设置难度
    /// </summary>
    public void SetModeAndPlay(int modeIndex)
    {
        LevelMode mode = (LevelMode)modeIndex;
        GameDataUtils.Instance.LoadLevel(mode);
        StartNewGame();
    }

    /// <summary>
    /// 显示选关面板
    /// </summary>
    public void ShowSelectLevelPanel()
    {
        selectPanel.SetActive(true);
        selectPanel.transform.DOMoveY(-350, 0.2f);
        mask.DOFade(0.9f, 0.2f);
        mask.raycastTarget = true;
    }

    /// <summary>
    /// 关闭选关面板
    /// </summary>
    public void CloseSelectLevelPanel(int customDuration = -1)
    {
        mask.DOFade(0, customDuration != -1 ? customDuration : 0.2f);
        selectPanel.transform.DOMoveY(-1500, customDuration != -1 ? customDuration : 0.2f).OnComplete(() =>
        {
            selectPanel.SetActive(false);
            mask.raycastTarget = false;
        });
    }
    
    /// <summary>
    /// 开始游戏
    /// </summary>
    private void StartNewGame()
    {
        GameDataUtils.Instance.isGaming = true;
        GameDataUtils.Instance.solitaireScene.resultGroup.gameObject.SetActive(false);
        
        // 创建动画序列
        Sequence sequence = DOTween.Sequence();

        CloseSelectLevelPanel(0);
        gameBg.raycastTarget = true;
        sequence.Join(gameBg.DOFade(1f, 0.2f));

        sequence.OnComplete(() =>
        {
            GameDataUtils.Instance.solitaireScene.InitGame();
        });
    }

    /// <summary>
    /// 继续游戏
    /// </summary>
    public void ContinueGame()
    {
        GameDataUtils.Instance.isGaming = true;
        GameDataUtils.Instance.savedState = GameStateManager.Instance.LoadSavedGameState();
        GameDataUtils.Instance.solitaireScene.LoadGame(GameDataUtils.Instance.savedState);
        
        // 创建动画序列
        Sequence sequence = DOTween.Sequence();

        CloseSelectLevelPanel(0);
        gameBg.raycastTarget = true;
        sequence.Join(gameBg.DOFade(1f, 0.2f));

        sequence.OnComplete(() =>
        {
            GameDataUtils.Instance.solitaireScene.SetAllGameGroupActive(true);
        });
    }

    /// <summary>
    /// 清除存档
    /// </summary>
    public void ClearSaveData()
    {
        GameStateManager.Instance.ClearSavedGameState();
        LevelData.ResetLevelData();
        SetContinueButton();
        UpdateText();
    }
}
