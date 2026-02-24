using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    [Header("基本信息")]
    private LevelMode currentLevelMode; // 当前难度
    public Dictionary<string, CardGroup> cardGroupsDict = new Dictionary<string, CardGroup>(); // 所有牌
    public int aimCount; // 目标数量
    public int completeCount; // 完成数量
    public bool isMovingCard = false; // 是否正在移动牌
    
    [Header("预制体")]
    public GameObject cardGroupPrefab; // 牌组预制体
    public GameObject characterCardPrefab; // 文字卡预制体
    public GameObject categoryCardPrefabs; // 种类卡预制体

    [Header("发牌相关")]
    public GameObject cardSpawnPoint; // 卡牌生成点
    public float waitForStartTime = 1f; // 发牌前的等待时间
    public float DealingInterval; // 发牌间隔时间
    public bool isInitializing = false; // 是否正在初始化

    [Header("组件")]
    public StackArea stackArea; // 叠牌区
    public BaseArea baseArea; // 基础区
    public TakeArea takeArea; // 盖牌区
    public ProgressBar progressBar; // 进度条
    public Timer timer; // 计时器
    public ResultPanel resultPanel; // 结果面板
    
    [Header("关卡数据")]
    private GameplayLevelData levelData; // 关卡数据
    private int baseSlotCnt; // 基础区的卡槽数量
    private int stackSlotCnt; // 叠牌区的卡槽数量
    private int allStackCardCount; // 叠牌区要发的卡牌张数
    private int score; // 得分
    

    /// <summary>
    /// 初始化游戏
    /// </summary>
    public void InitGame()
    {
        isInitializing = true;

        aimCount = levelData.KVPs.Count; // 获取目标数量（卡牌种类总数）
        stackSlotCnt = levelData.OperateSlotCnt; // 获取叠牌区的卡槽数量
        baseSlotCnt = levelData.TargetSlotCnt; // 获取基础区的卡槽数量
        
        // 初始化卡槽
        stackArea.Init(stackSlotCnt, levelData.OperateSlots);
        baseArea.Init(baseSlotCnt, levelData.TargetSlots);
        takeArea.Init();
        
        // 初始化进度条
        progressBar.Init(aimCount);
        
        // 生成卡牌
        GenerateCard();
    }

    /// <summary>
    /// 初始化游戏数据
    /// </summary>
    /// <param name="levelData"></param>
    public void InitGame(GameplayLevelData levelData)
    {
        // 获取关卡数据
        this.levelData = levelData;
    }

    /// <summary>
    /// 生成卡牌
    /// </summary>
    private void GenerateCard()
    {
        for (int i = 0; i < aimCount; i++)
        {
            string category = levelData.KVPs[i].Category;
            
            List<string> words = new List<string>(levelData.KVPs[i].Words);
            
            // 生成分类卡
            CardGroup cardGroup = Instantiate(cardGroupPrefab, cardSpawnPoint.transform.position, Quaternion.identity).GetComponent<CardGroup>();
            CategoryCard categoryCard = Instantiate(categoryCardPrefabs, cardSpawnPoint.transform.position, Quaternion.identity).GetComponent<CategoryCard>();
            categoryCard.transform.parent = cardGroup.transform;
            // 初始化
            categoryCard.Init(CardType.Category, category, category, levelData.KVPs[i].Words.Count);
            cardGroup.Init();
            // 添加到字典中
            cardGroupsDict.Add(category, cardGroup);
            
            // 生成文字卡
            foreach (string value in words)
            {
                cardGroup = Instantiate(cardGroupPrefab, cardSpawnPoint.transform.position, Quaternion.identity).GetComponent<CardGroup>();
                CharacterCard characterCard = Instantiate(characterCardPrefab, cardSpawnPoint.transform.position, Quaternion.identity).GetComponent<CharacterCard>();
                characterCard.transform.parent = cardGroup.transform;
                // 初始化
                characterCard.Init(CardType.Character, category, value);
                cardGroup.Init();
                // 添加到字典中
                cardGroupsDict.Add(value, cardGroup);
            }
        }

        // 发牌
        StartCoroutine(DealCards());
    }

    /// <summary>
    /// 发牌
    /// </summary>
    private IEnumerator DealCards()
    {
        for (int i = 0; i < levelData.TargetSlotCnt; i++)
        {
            List<CardGroup> cardGroups = new List<CardGroup>();
            
            foreach (var cardName in levelData.TargetSlots[i].Infos)
            {
                CardGroup cardGroup = cardGroupsDict[cardName];
                
                cardGroups.Add(cardGroup);
            }

            foreach (var cardGroup in cardGroups)
            {
                Vector3 cardPos = cardGroup.DealCardToBaseSlotOnStart(baseArea.slots[i]);
                cardGroup.transform.position = cardPos;
                
                // 创建进度点
                if (baseArea.slots[i].allCards.Count > 1)
                {
                    if (baseArea.slots[i].allCards.Count < baseArea.slots[i].cardCount + 1)
                    {
                        // 创建进度点
                        if (!baseArea.slots[i].categoryCard.isTextScale)
                        {
                            baseArea.slots[i].CreateProgressPoints();
                        }

                        // 点亮进度点
                        if (baseArea.slots[i].progressPointSrs.Count > 0)
                        {
                            baseArea.slots[i].LightUpProgressPoint();
                        }
                    }
                    else
                    {
                        // 移除进度条
                        baseArea.slots[i].RemoveProgressPoints();
                    }
                }

                // 调整分类卡的位置
                if (baseArea.slots[i].allCards.Count > 1 && !baseArea.slots[i].categoryCard.isTextScale)
                {
                    baseArea.slots[i].categoryCard.MovingCardTextToSmall(0);
                    baseArea.slots[i].categoryCard.MovingCardPos(0);
                }
            }
        }
        
        // 发牌前等待
        yield return new WaitForSeconds(waitForStartTime);
        
        // 计算叠牌区的初始卡牌数量
        allStackCardCount += levelData.OperateSlots[^1].Infos.Count * stackSlotCnt;
        
        // 向叠牌区发牌
        for (int i = 0; i < allStackCardCount; i++)
        {
            StackAreaSlot currentSlot = stackArea.slots[i % stackSlotCnt];

            if (currentSlot.allCards.Count < currentSlot.cardInfos.Count)
            {
                // 取出一个牌组
                CardGroup cardGroup = cardGroupsDict[currentSlot.cardInfos[i / stackSlotCnt]];
                
                // 添加到卡槽中
                Vector3 cardPos = cardGroup.DealCardToStackSlotOnStart(currentSlot);
                
                // 执行移动位置动画
                cardGroup.transform.DOMove(cardPos, 0.2f)
                    .SetEase(Ease.InOutQuad);

                yield return new WaitForSeconds(DealingInterval);
            }
        }
        
        // 将剩余的牌移动到取牌区
        MovingLeftCardsToTakeArea();

        SetTimerStartOrStop(true);
        
        isInitializing = false;
    }
    
    /// <summary>
    /// 将剩余的牌移动到取牌区
    /// </summary>
    private void MovingLeftCardsToTakeArea()
    {
        for (int i = 0; i < levelData.HomeSlot.Infos.Count; i++)
        {
            string cardName = levelData.HomeSlot.Infos[i];
            
            takeArea.AddCard(cardGroupsDict[cardName]);
        }
        
        takeArea.UpdateCardsPos();
    }

    /// <summary>
    /// 设置进度条
    /// </summary>
    public void SetProgress()
    {
        progressBar.SetProgress(completeCount, aimCount);


        if (completeCount == aimCount)
        {
            StartCoroutine(CheckComplete());
        }
    }

    /// <summary>
    /// 设置计时器开始和暂停
    /// </summary>
    public void SetTimerStartOrStop(bool state)
    {
        timer.isStart = state;
    }

    /// <summary>
    /// 检查是否完成
    /// </summary>
    public IEnumerator CheckComplete()
    {
        yield return new WaitForSeconds(0.2f);
        
        resultPanel.gameObject.SetActive(true);
        
        resultPanel.SetResult(timer.time, (int)levelData.Mode, score);
        
        SetTimerStartOrStop(false);
        
        LevelData.levelIdDict[levelData.Mode]++;
    }

    /// <summary>
    /// 清空游戏版面
    /// </summary>
    public void ClearGameBoard()
    {
        // 清空基础区
        foreach (var slot in baseArea.slots)
        {
            foreach (var cardGroup in slot.allCardGroups)
            {
                Destroy(cardGroup.gameObject);
            }
            Destroy(slot.gameObject);
        }
        baseArea.slots.Clear();

        // 清空叠牌区
        foreach (var slot in stackArea.slots)
        {
            foreach (var cardGroup in slot.allCardGroups)
            {
                Destroy(cardGroup.gameObject);
            }
            Destroy(slot.gameObject);
        }
        stackArea.slots.Clear();
        
        // 清空取牌区
        Destroy(takeArea.takeAreaSlot);
        
        takeArea.cardGroups.Clear();
    }
}
