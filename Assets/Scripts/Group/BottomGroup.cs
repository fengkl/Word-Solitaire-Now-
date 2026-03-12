using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BottomGroup : MonoBehaviour
{
    public Image homeBg;
    public Image homeIcon;
    
    public Sprite normalIconSprite;
    public Sprite dailyIconSprite;
    
    public void ReplaceNormalStyle()
    {
        homeBg.color = new Color(28 / 255f, 171 / 255f, 64 / 255f, 1);
        // homeIcon.sprite = normalIconSprite;
    }
    
    public void ReplaceDailyStyle()
    {
        homeBg.color = new Color(22 / 255f, 126 / 255f, 220 / 255f, 1);
        // homeIcon.sprite = dailyIconSprite;
    }
}
