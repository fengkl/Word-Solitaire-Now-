using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePreferences {
    public static int DailyLevelIndex {
        get {
            return PlayerPrefs.GetInt("DailyLevelIndex", 0);
        }
        set {
            PlayerPrefs.SetInt("DailyLevelIndex", value);
        }
    }

}
