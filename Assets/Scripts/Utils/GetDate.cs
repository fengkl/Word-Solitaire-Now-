using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetDate
{
    /// <summary>
    /// 得到月份的天数
    /// </summary>
    public static int GetMonthDays(int year, int month)
    {
        return DateTime.DaysInMonth(year, month);
    }
    
    /// <summary>
    /// 得到天数为礼拜几
    /// </summary>
    public static int GetDayOfWeek(int year, int month, int day)
    {
 
        DateTime dt = new DateTime(year, month, day);
      
        switch (dt.DayOfWeek.ToString())
        {
            case "Monday":return 2;
            case "Tuesday": return 3;
            case "Wednesday": return 4;
            case "Thursday": return 5;
            case "Friday": return 6;
            case "Saturday": return 7;
            case "Sunday": return 1;
        }
        
        return 0;
    }
}
