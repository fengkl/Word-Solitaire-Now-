using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayUtils  {
    public static int CalculateRandomSeed(int mode, int levelId) {
        // 两个大质数用于打散数据
        const int PRIME_1 = 19349663;
        const int PRIME_2 = 73856093;

        // 异或操作混合两个维度的影响
        return (PRIME_1 * levelId) ^ (PRIME_2 * mode);
    }
    public static string GetCardLangInfoKey(string info) {
        return "v_" + info;
    }
}
