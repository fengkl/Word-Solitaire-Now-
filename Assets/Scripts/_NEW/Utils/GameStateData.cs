using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏状态
/// </summary>
public class GameState
{
    // 游戏基本状态
    public GameplayLevelData levelData; // 关卡数据
    public LevelMode gameMode; // 难度
    public int levelId; // 关卡ID
    public int randomSeed; // 随机种子
    public float gameTime; // 游戏时间
    
    // 卡牌状态列表
    public List<CardState> cardStates = new List<CardState>();
    
    // 槽位状态
    public List<SlotState> slotStates = new List<SlotState>();
    
    // 统计信息
    public int completeCount; // 已完成牌组数量
    public int aimCount; // 目标牌组数量
}

/// <summary>
/// 卡牌状态
/// </summary>
public class CardState
{
    public string cardName; // 卡牌名称
    public string cardCategory; // 卡牌分类
    public CardType cardType; // 卡牌类型
    public bool isFaceDown; // 是否盖着
    public bool canSelected; // 是否可选
    public Vector3 position; // 位置
    public Vector3 scale; // 缩放大小
    public string currentSlotId; // 当前所在的卡槽ID
}

/// <summary>
/// 槽位状态
/// </summary>
public class SlotState
{
    public string slotId; // 卡槽ID
    public SlotType slotType; // 卡槽类型
    public List<string> cardNames = new List<string>(); // 卡槽内卡牌名称列表
    public Vector3 position; // 位置
}