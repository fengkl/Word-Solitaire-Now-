using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CardData
{
    public static Dictionary<string, List<string>> categories = new Dictionary<string, List<string>>()
    {
        // { "test1", new List<string> { "test1-1", "test1-2", "test1-3" } },
        // { "test2", new List<string> { "test2-1", "test2-2" } },
        // { "test3", new List<string> { "test3-1", "test3-2", "test3-3", "test3-4" } },
        // { "test4", new List<string> { "test4-1", "test4-2", "test4-3", "test4-4", "test4-5" } },
        // { "test5", new List<string> { "test5-1", "test5-2", "test5-3", "test5-4" } },


        // { "动物", new List<string> { "狮子", "老虎", "熊猫", "长颈鹿", "大象", "猴子", "兔子", "狗", "猫" } },
        // { "水果", new List<string> { "苹果", "香蕉", "橘子", "草莓" } },
        // { "驾驶", new List<string> { "交通", "道路", "公路" } },
        // { "火花", new List<string> { "闪光", "微光", "闪烁" } },
        // { "射箭", new List<string> { "箭尾", "长弓", "射箭场", "靶子", "箭袋", "箭羽" } },
        // { "罗马", new List<string> { "广场", "宫殿", "雷穆斯", "意大利" } },
        // { "脚踝", new List<string> { "脚后跟", "脚掌", "扭伤", "袜子", "护具", "关节", "脚链", "靴子" } },
        // { "军刀", new List<string> { "重剑", "花剑", "刺剑", "阔刃大刀" } },
        // { "果酱", new List<string> { "罐子", "三明治", "果胶", "吐司", "派" } },
        // { "记忆", new List<string> { "回想", "背诵", "记住" } },
        // { "冬天", new List<string> { "冰冷", "严寒", "寒冷", "凉飕飕" } },
        // { "恒星", new List<string> { "北极星", "参宿七", "牛郎星", "天狼星", "织女星" } },
        // { "神兽", new List<string> { "克拉肯", "狮鹫", "九头蛇", "独角兽", "天马", "半人马", "凤凰" } },
        
        { "Animals", new List<string> { "Lion", "Tiger", "Panda", "Giraffe", "Elephant", "Monkey", "Rabbit", "Dog", "Cat" } },
        { "Fruits", new List<string> { "Apple", "Banana", "Orange", "Strawberry", "Grape", "Watermelon", "Pineapple" } },
        { "Driving", new List<string> { "Traffic", "Road", "Highway", "Vehicle", "Speed" } },
        { "Memory", new List<string> { "Recall", "Memorize", "Remember" } },
        { "Jam", new List<string> { "Jar", "Sandwich", "Pie" } },
        { "Winter", new List<string> { "Icy", "Cold", "Freezing" } },
        { "Stationery", new List<string> { "Pen", "Pencil", "Eraser", "Ruler", "Notebook", "Scissors", "Glue" } },
        { "Furniture", new List<string> { "Sofa", "Bed", "Table", "Chair", "Bookshelf", "Desk" } },
        { "Level", new List<string> { "Mid", "High", "Low" } },
        { "Weather", new List<string> { "Storm", "Snow", "Rain", "Sunny" } },
    };
}