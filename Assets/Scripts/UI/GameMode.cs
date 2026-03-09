using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMode : MonoBehaviour {
    //public Text modeText;
    public Text levelText;

    public void SetModeText(string mode, int level) {
        //modeText.text = mode;
        levelText.text = "Level " + level;
    }
}
