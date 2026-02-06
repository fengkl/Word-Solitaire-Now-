using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestFunction : MonoBehaviour {
    [UnityEditor.MenuItem("Tools/DecryptLevel", false, 107)]
    public static void DecryptVitaMetaData() {
        LevelFileHandler levelFileHandler = new LevelFileHandler();
        levelFileHandler.Init();
        levelFileHandler.LoadAllLevels();

        GameplayLevelData level = new GameplayLevelData();

        levelFileHandler.TryGetLevelData(LevelMode.Easy, 1, 0, out level);

        Debug.Log(level);
    }
}
