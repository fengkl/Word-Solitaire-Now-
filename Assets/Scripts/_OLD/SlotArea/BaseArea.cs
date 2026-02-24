using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseArea : MonoBehaviour
{
    public int targetSlotCnt; //目标卡槽数量
    public List<BaseAreaSlot> slots; //卡槽索引
    public float interval; //卡槽间隔
    
    public GameObject slotPrefab; //卡槽预制体

    /// <summary>
    /// 初始化卡槽
    /// </summary>
    public void Init(int count, List<WordSlotInfo> infos)
    { 
        targetSlotCnt = count;
        
        // 清空基础区
        foreach (var slot in slots)
        {
            foreach (var cardGroup in slot.allCardGroups)
            {
                Destroy(cardGroup.gameObject);
            }
            Destroy(slot.gameObject);
        }
        slots.Clear();
        
        // 计算总宽度和起始位置（中心对称布局）
        float totalWidth = interval * (targetSlotCnt - 1); // 总宽度
        float startX = -totalWidth / 2f; // 最左边的起始位置
        
        for (int i = 0; i < targetSlotCnt; i++)
        {
            // 计算位置
            float xPos = startX + (interval * i);
            Vector3 slotPosition = new Vector3(xPos, 0, 0);
        
            // 创建卡槽
            BaseAreaSlot slot = Instantiate(slotPrefab, transform).GetComponent<BaseAreaSlot>();
            slot.transform.localPosition = slotPosition;
            slot.transform.localRotation = Quaternion.identity;
            slot.cardInfos = infos[i].Infos;
            slots.Add(slot);
        }
    }
}
