using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackGroup : MonoBehaviour
{
    public float interval; //卡槽间隔
    
    public GameObject slotPrefab; //卡槽预制体
    
    /// <summary>
    /// 初始化卡槽
    /// </summary>
    public void Init(int count, List<WordSlotInfo> infos)
    { 
        // 清空叠牌区
        GameDataUtils.Instance.operateSlots.Clear();
        
        // 计算总宽度和起始位置（中心对称布局）
        float totalWidth = interval * (count - 1); // 总宽度
        float startX = -totalWidth / 2f; // 最左边的起始位置
        
        for (int i = 0; i < count; i++)
        {
            // 计算位置
            float xPos = startX + (interval * i);
            Vector3 slotPosition = new Vector3(xPos, 0, 0);
        
            // 创建卡槽
            OperateSlot slot = Instantiate(slotPrefab).GetComponent<OperateSlot>();
            slot.transform.SetParent(transform);
            slot.transform.localPosition = slotPosition;
            slot.slotId = i + 10;
            slot.slotType = SlotType.OperateSlot;
            slot.cardInfos = infos[i].Infos;
            
            GameDataUtils.Instance.operateSlots.Add(slot);
        }
    }
}
