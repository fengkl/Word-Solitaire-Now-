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

    public CardGroup currentPickedCardGroup; // 当前拿着的牌组
    
    [Header("基本信息")]
    public List<CardGroup> cardGroups = new List<CardGroup>(); // 所有牌
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
    public int rowCount = 2; // 行数
    public List<Vector3> cardPoss = new List<Vector3>(); // 卡牌位置
    public bool isInitializing = false; // 是否正在初始化

    [Header("组件")]
    public StackArea stackArea; // 叠牌区
    public BaseArea baseArea; // 基础区
    public TakeArea takeArea; // 盖牌区
    public ProgressBar progressBar; // 进度条
    public Timer timer; // 计时器

    /// <summary>
    /// 初始化游戏
    /// </summary>
    public void InitGame()
    {
        isInitializing = true;
        
        // 生成卡牌
        GenerateCard();
        // 初始化卡槽
        stackArea.Init();
        baseArea.Init();
        takeArea.Init();
        // 初始化进度条
        progressBar.Init(aimCount);
    }

    /// <summary>
    /// 生成卡牌
    /// </summary>
    private void GenerateCard()
    {
        // 先选取种类
        List<string> keys = new List<string>(CardData.categories.Keys);
        
        // 洗牌
        ShuffleList(keys);

        for (int i = 0; i < aimCount; i++)
        {
            string category = keys[i];
            
            // 生成分类卡
            CardGroup cardGroup = Instantiate(cardGroupPrefab, cardSpawnPoint.transform.position, Quaternion.identity).GetComponent<CardGroup>();
            CategoryCard categoryCard = Instantiate(categoryCardPrefabs, cardSpawnPoint.transform.position, Quaternion.identity).GetComponent<CategoryCard>();
            categoryCard.transform.parent = cardGroup.transform;
            categoryCard.Init(CardType.Category, category, category, CardData.categories[category].Count);
            cardGroup.Init();
            cardGroups.Add(cardGroup);
            
            // 生成文字卡
            foreach (string value in CardData.categories[category])
            {
                cardGroup = Instantiate(cardGroupPrefab, cardSpawnPoint.transform.position, Quaternion.identity).GetComponent<CardGroup>();
                CharacterCard characterCard = Instantiate(characterCardPrefab, cardSpawnPoint.transform.position, Quaternion.identity).GetComponent<CharacterCard>();
                characterCard.transform.parent = cardGroup.transform;
                characterCard.Init(CardType.Character, category, value);
                cardGroup.Init();
                cardGroups.Add(cardGroup);
            }
        }
        
        // 再次洗牌
        ShuffleList(cardGroups);

        // 发牌
        StartCoroutine(DealCards());
    }

    /// <summary>
    /// 发牌
    /// </summary>
    private IEnumerator DealCards()
    {
        // 发牌前等待
        yield return new WaitForSeconds(waitForStartTime);
        
        int stepsCount = 0; // 梯形阶数计数器
        int offsetCount = 0; // 偏移计数器
        int slotCount = stackArea.slotCount; // 卡槽数
        int otherCount = slotCount * (1 + slotCount) / 2 - slotCount; // 梯形部分卡牌数(等差数列求前n项和)
        int cardCount = slotCount * rowCount + otherCount; // 发出的卡牌数 = 卡槽数 * 行数 + 梯形部分卡牌数
        
        // 开始发牌
        for (int i = 0; i < cardCount; i++)
        {
            // 取出一个牌组
            CardGroup cardGroup = cardGroups[i];
            // 计算卡槽索引
            int slotIndex = (i + stepsCount) % slotCount;
            
            if (i >= cardCount - otherCount && slotIndex == 0)
            {
                offsetCount++;
                stepsCount += offsetCount;
                slotIndex = (i + stepsCount) % slotCount;
            }
            
            // // 获取卡槽
            StackAreaSlot stackAreaSlot = stackArea.slots[slotIndex];
            
            // 添加到卡槽中
            cardPoss.Add(cardGroup.DealCardOnStart(stackAreaSlot));
        }

        for (int i = 0; i < cardCount; i++)
        {
            // 创建序列动画
            Sequence sequence = DOTween.Sequence();
            
            // 取出一个牌组
            CardGroup cardGroup = cardGroups[i];
            // 得到位置
            Vector3 cardPos = cardPoss[i];
            
            // 执行移动位置动画
            sequence.Join(cardGroup.transform.DOMove(cardPos, 0.2f))
                .SetEase(Ease.InOutQuad);

            yield return new WaitForSeconds(DealingInterval);
        }
        
        // 将剩余的牌移动到取牌区
        MovingLeftCardsToTakeArea(cardCount);

        SetTimerStartOrStop(true);
        
        isInitializing = false;
    }

    /// <summary>
    /// 洗牌
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// <summary>
    /// 将剩余的牌移动到取牌区
    /// </summary>
    private void MovingLeftCardsToTakeArea(int cardIndex)
    {
        for (int i = cardIndex; i < cardGroups.Count; i++)
        {
            takeArea.AddCard(cardGroups[i]);
        }
        
        takeArea.UpdateCardsPos();
    }

    /// <summary>
    /// 设置进度条
    /// </summary>
    public void SetProgress()
    {
        progressBar.SetProgress(completeCount, aimCount);
    }
    
    /// <summary>
    /// 设置计时器开始和暂停
    /// </summary>
    public void SetTimerStartOrStop(bool state)
    {
        timer.isStart = state;
    }
}
