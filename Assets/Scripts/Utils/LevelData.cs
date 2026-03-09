using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelData : MonoBehaviour {
    public static int currentEasyLevelId = 1; // 当前简单关卡id
    public static int currentMediumLevelId = 1; // 当前普通关卡id
    public static int currentHardLevelId = 1; // 当前困难关卡id

    public static Dictionary<LevelMode, int> levelIdDict = new Dictionary<LevelMode, int>() {
        { LevelMode.Easy, currentEasyLevelId },
        { LevelMode.Normal, currentMediumLevelId },
        { LevelMode.Hard, currentHardLevelId }
    };

    /// <summary>
    /// 重置存档
    /// </summary>
    public static void ResetLevelData() {
        levelIdDict[LevelMode.Easy] = 1;
        levelIdDict[LevelMode.Normal] = 1;
        levelIdDict[LevelMode.Hard] = 1;
    }
}
