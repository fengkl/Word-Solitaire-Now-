using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 
    StackArea : MonoBehaviour
{
    public int slotCount; //卡槽数量
    public List<StackAreaSlot> slots; //卡槽索引
    public float interval; //卡槽间隔
    
    public GameObject slotPrefab; //卡槽预制体
    
    /// <summary>
    /// 初始化卡槽
    /// </summary>
    public void Init()
    { 
        transform.position += new Vector3(-(interval / 2) * (slotCount - 1), 0, 0);
        
        for (int i = 0; i < slotCount; i++)
        {
            GameObject slot = Instantiate(slotPrefab, transform.position + new Vector3(interval * i, 0, 0), Quaternion.identity);
            slot.transform.parent = transform;
            slots.Add(slot.GetComponent<StackAreaSlot>());
        }
    }
}
