using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CalenderGroup : MonoBehaviour
{
    public int curYear;
    public int curMonth;
    public int curDay;
    
    [Header("日期")]
    public Text yearMonthText;
    public Transform dayList;
    public List<GameObject> dayNodesList = new List<GameObject>();
    
    [Header("预制体")]
    public GameObject dayPrefab;
    
    [Header("左右箭头")]
    public GameObject leftArrow;
    public GameObject rightArrow;
    
    [Header("当前选中的按钮")]
    public GameObject selectBtn;
    
    private Image curBtnImage;
    private Color curColor;
 
    void Start()
    {
        DateTime nowDateTime = DateTime.Now;     
        curYear = nowDateTime.Year;
        curMonth = nowDateTime.Month;
        curDay = nowDateTime.Day;
        UpdateYearMonth();
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 初始化日历
    /// </summary>
    public void Init(int year, int month, int day)
    {
        curYear = year;
        curMonth = month;
        curDay = day;
        
        Debug.Log("初始化日历" + year + "年" + month + "月" + day + "日");
        Debug.Log("初始化日历" + curYear + "年" + curMonth + "月" + curDay + "日");
    }

    /// <summary>
    /// 更新年月
    /// </summary>
    private void UpdateYearMonth()
    {
        yearMonthText.text = curYear + "." + curMonth;
    }
 
    private void ShowDayList(int year, int month)
    {
        ClearCalender(dayList);
        UpdateYearMonth();
 
        int days = GetDate.GetMonthDays(year, month);// 得到天数
        int week = GetDate.GetDayOfWeek(year, month, 1); // 得到每月一号是星期几 
        
        // 生成空的占位节点
        for (int i = 1; i < week; i++)
        {
            GameObject dayNode = InstantiateItemSetParent(dayPrefab, dayList);
            dayNode.transform.Find("Text").GetComponent<Text>().text = "";
        }
        
        // 根据当前月份的天数来生成节点
        for (int i = 1; i <= days; i++)
        {
            GameObject dayNode = InstantiateItemSetParent(dayPrefab, dayList);
            
            Text txt = dayNode.transform.Find("Text").GetComponent<Text>();

            txt.text = i.ToString();

            if (new DateTime(year, month, i) < DateTime.Now)
            {
                txt.color = Color.black;
                
                Button btn = dayNode.GetComponent<Button>();
            
                btn.onClick.AddListener(() =>
                {
                    SelectDate(btn.gameObject);
                });
                
                rightArrow.SetActive(true);
            }
            else
            {
                txt.color = Color.gray;
                rightArrow.SetActive(false);
            }

            if (LevelData.dailyLevelIdSet.Contains(curYear * 10000 + month * 100 + i))
            {
                dayNode.transform.Find("Medal").gameObject.SetActive(true);
            }
            
            dayNodesList.Add(dayNode);
        }

        // 如果是当前年月，则选中当前日期
        if (curYear == DateTime.Now.Year && curMonth == DateTime.Now.Month)
        {
            SelectDate(dayNodesList[DateTime.Now.Day - 1].gameObject);
        }
        // 如果不是当前年月，则选中第一天
        else
        {
            SelectDate(dayNodesList[0].gameObject);
        }
        
        Debug.Log("日历" + curYear + "年" + curMonth + "月" + curDay + "日");
    }
    
    /// <summary>
    /// 前一月
    /// </summary>
    public void UpMonth()
    {
        if (curMonth - 1 == 0)
        {
            curYear -= 1;
            curMonth = 12;
        }
        else
        {
            curMonth -= 1;
        }
      
        ShowDayList(curYear, curMonth);
    }
    
    /// <summary>
    /// 后一月
    /// </summary>
    public void DownMonth()//往后
    {
        if (curMonth + 1 == 13)
        {
            curYear += 1;
            curMonth = 1;
        }
        else
        {
            curMonth += 1;
        }
      
        ShowDayList(curYear, curMonth);
    }
    
    /// <summary>
    /// 清空日历
    /// </summary>
    /// <param name="pos"></param>
    private void ClearCalender(Transform pos)
    {
        selectBtn = null;
        dayNodesList.Clear();
        
        for (int i = 0; i < pos.childCount; i++)
        {
            Destroy(pos.GetChild(i).gameObject);
        }
    }
    
    /// <summary>
    /// 实例化节点
    /// </summary>
    private GameObject InstantiateItemSetParent(GameObject item, Transform parent)
    {
        GameObject dayNode = Instantiate(item, parent, true);
        dayNode.transform.localPosition = Vector3.zero;
        dayNode.transform.localScale = Vector3.one;
        dayNode.transform.localEulerAngles = Vector3.zero;
        
        return dayNode;
    }

    /// <summary>
    /// 选择日期
    /// </summary>
    private void SelectDate(GameObject button)
    {
        if (selectBtn != null)
        {
            selectBtn.transform.Find("Image").GetComponent<Image>().gameObject.SetActive(false);
            selectBtn.transform.Find("Text").GetComponent<Text>().color = Color.black;

        }
        
        button.transform.Find("Image").GetComponent<Image>().gameObject.SetActive(true);
        button.transform.Find("Text").GetComponent<Text>().color = Color.white;
        curDay = int.Parse(button.transform.Find("Text").GetComponent<Text>().text);
        
        selectBtn = button;
    }
    
    /// <summary>
    /// 根据日期获取关卡ID
    /// </summary>
    public int GetSelectDayLevelID()
    {
        return curYear * 10000 + curMonth * 100 + curDay;
    }
    
    //===================================================================
    //                           按钮响应方法
    //===================================================================
    
    public void ShowCalender(bool anim = true)
    {
        gameObject.SetActive(true);
        ShowDayList(curYear, curMonth);
        gameObject.transform.DOMoveY(100, anim ? 0.2f : 0f);
    }
    
    public void CloseCalender(bool anim = true)
    {
        gameObject.transform.DOMoveY(-1500, anim ? 0.2f : 0f)
            .OnComplete(() => {
                gameObject.SetActive(false);
            });
    }
}
