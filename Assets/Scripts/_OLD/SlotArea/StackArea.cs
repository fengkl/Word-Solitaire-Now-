using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackArea : MonoBehaviour
{
    public int operateSlotCnt; // 操作卡槽数量
    public List<StackAreaSlot> slots; //卡槽索引
    public float interval; //卡槽间隔
    
    public GameObject slotPrefab; //卡槽预制体
    
    /// <summary>
    /// 初始化卡槽
    /// </summary>
    public void Init(int count, List<WordSlotInfo> infos)
    { 
        // 清空叠牌区
        foreach (var slot in slots)
        {
            foreach (var cardGroup in slot.allCardGroups)
            {
                Destroy(cardGroup.gameObject);
            }
            Destroy(slot.gameObject);
        }
        slots.Clear();
        
        operateSlotCnt = count;
        
        // 计算总宽度和起始位置（中心对称布局）
        float totalWidth = interval * (operateSlotCnt - 1); // 总宽度
        float startX = -totalWidth / 2f; // 最左边的起始位置
        
        for (int i = 0; i < operateSlotCnt; i++)
        {
            // 计算位置
            float xPos = startX + (interval * i);
            Vector3 slotPosition = new Vector3(xPos, 0, 0);
        
            // 创建卡槽
            StackAreaSlot slot = Instantiate(slotPrefab, transform).GetComponent<StackAreaSlot>();
            slot.transform.localPosition = slotPosition;
            slot.transform.localRotation = Quaternion.identity;
            slot.cardInfos = infos[i].Infos;
            slots.Add(slot);
        }
    }
}
