using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Security.Cryptography;
using System.Text;
using System;

public class Utils : MonoBehaviour {

    public static byte[] DecryptAes(byte[] bytes) {
        // 1. 创建 AES 实例
        using (Aes aes = Aes.Create()) {
            // 2. 设置密钥 (Key)
            // 对应代码中的 StringLiteral_12721
            aes.Key = Encoding.UTF8.GetBytes("WaterTiger12358=");

            // 3. 设置加密模式 (Mode)
            // 参数 1 对应 CipherMode.CBC
            aes.Mode = CipherMode.CBC;

            // 4. 设置填充模式 (Padding)
            // 参数 2 对应 PaddingMode.PKCS7
            aes.Padding = PaddingMode.PKCS7;

            // 5. 设置初始化向量 (IV)
            // 对应代码中的 StringLiteral_5928
            aes.IV = Encoding.UTF8.GetBytes("FireRhino786532+");

            // 6. 创建解密器
            using (ICryptoTransform decryptor = aes.CreateDecryptor()) {
                // 7. 执行解密
                // IL2CPP 中数组长度通常在偏移 12 (0xC) 的位置
                return decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            }
        }
    }

    public static List<T> GetRandomList<T>(List<T> sourceList, int count, int seed) {
        // 1. 如果请求数量无效，返回空列表
        if (count <= 0)
            return new List<T>();

        // 2. 确定实际要取的数量 (不能超过源列表长度)
        int actualCount = Math.Min(sourceList.Count, count);

        // 3. 复制源列表 (避免修改原数据)
        List<T> copyList = new List<T>(sourceList);

        // 4. 初始化随机数生成器
        System.Random random = new System.Random(seed);

        // 5. Fisher-Yates 洗牌 (只进行前 actualCount 次交换)
        // 这样前 actualCount 个元素就是随机且打乱的
        for (int i = 0; i < actualCount; i++) {
            // 随机选取一个位置，范围是 [i, TotalCount)
            // 保证当前位置 i 之后(包含 i) 的所有元素都有机会被换到位置 i
            int randomIndex = random.Next(i, copyList.Count);

            // 交换 copyList[i] 和 copyList[randomIndex]
            T temp = copyList[randomIndex];
            copyList[randomIndex] = copyList[i];
            copyList[i] = temp;
        }

        // 5. 手动实现 Take(actualCount).ToList()
        // 因为 copyList 此时长度仍然是 sourceList.Count，我们只需要前 actualCount 个
        List<T> resultList = new List<T>(actualCount);
        for (int i = 0; i < actualCount; i++)
            resultList.Add(copyList[i]);

        return resultList;
    }
}
