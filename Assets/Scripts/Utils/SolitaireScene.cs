using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SolitaireScene : MonoBehaviour
{
    public Image bg;
    public BaseGroup baseGroup;
    public StackGroup stackGroup;
    public TakeGroup takeGroup;
    public CardGroup_New cardGroup;
    public GameObject counterGroup;
    public GameObject highLightGroup;
    public GameObject progressPointGroup;
    public GameObject moveGroup;
    
    [Header("卡牌预制体")]
    public CategoryCardActor categoryCardPrefabs;
    public CharacterCardActor CharacterCardPrefabs;

    [Header("关卡数据")]
    private GameplayLevelData levelData; // 关卡数据
    private int aimCount; // 目标数量
    private int baseSlotCnt; // 基础区的卡槽数量
    private int stackSlotCnt; // 叠牌区的卡槽数量
    
    [Header("发牌相关")]
    public float waitForStartTime = 1f; // 发牌前的等待时间
    public float DealingInterval; // 发牌间隔时间
    private int allStackCardCount; // 叠牌区要发的卡牌张数
    

    private void Start()
    {
        GameDataUtils.Instance.solitaireScene = this;
        
        InitLevelData();
        InitGame();
    }
    
    private void InitLevelData()
    {
        GameDataUtils.Instance.levelHandler = new LevelFileHandler();
        GameDataUtils.Instance.loopLevelHandler = new LoopLevelFileHandler();
        GameDataUtils.Instance.levelHandler.Init();
        GameDataUtils.Instance.loopLevelHandler.Init();
        GameDataUtils.Instance.levelHandler.LoadAllLevels();
        GameDataUtils.Instance.loopLevelHandler.LoadAllLevels();
        
        Debug.Log("关卡初始化完成");
    }

    public void InitGame()
    {
        DisableScene();
        
        // 加载关卡
        GameDataUtils.Instance.LoadTestLevel();

        levelData = GameDataUtils.Instance.levelData; // 获取关卡数据
        aimCount = levelData.KVPs.Count; // 获取目标数量（卡牌种类总数）
        baseSlotCnt = levelData.TargetSlotCnt; // 获取基础区的卡槽数量
        stackSlotCnt = levelData.OperateSlotCnt; // 获取叠牌区的卡槽数量
        
        baseGroup.Init(baseSlotCnt, levelData.TargetSlots);
        stackGroup.Init(stackSlotCnt, levelData.OperateSlots);
        
        takeGroup.gameObject.SetActive(true);
        counterGroup.gameObject.SetActive(true);
        
        SetHighlightGroup();
        SetProgressPointGroup();
        
        GenerateCards();
    }

    public void GenerateCards()
    {
        for (int i = 0; i < aimCount; i++)
        {
            string category = levelData.KVPs[i].Category;
            
            List<string> words = new List<string>(levelData.KVPs[i].Words);
            
            // 生成分类卡
            CategoryCardActor categoryCard = Instantiate(categoryCardPrefabs).GetComponent<CategoryCardActor>();
            // 设置父物体
            categoryCard.transform.SetParent(cardGroup.transform);
            categoryCard.transform.position = takeGroup.homeSlot.transform.position;
            // 初始化分类卡
            categoryCard.Init(CardType.Category, category, category, levelData.KVPs[i].Words.Count);
            // 添加到数据字典中
            GameDataUtils.Instance.cardActors.Add(category, categoryCard);
            
            // 生成文字卡
            foreach (string word in words)
            {
                CharacterCardActor characterCard = Instantiate(CharacterCardPrefabs).GetComponent<CharacterCardActor>();
                // 设置父物体
                characterCard.transform.SetParent(cardGroup.transform);
                characterCard.transform.position = takeGroup.homeSlot.transform.position;
                // 初始化
                characterCard.Init(CardType.Character, category, word);
                // 添加到字典中
                GameDataUtils.Instance.cardActors.Add(word, characterCard);
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
        // 向基础区发牌
        for (int i = 0; i < levelData.TargetSlotCnt; i++)
        {
            TargetSlot targetSlot = GameDataUtils.Instance.targetSlots[i];
            List<CardActor> cards = new List<CardActor>();
            
            foreach (var cardName in levelData.TargetSlots[i].Infos)
            {
                CardActor card = GameDataUtils.Instance.cardActors[cardName];
                
                cards.Add(card);
            }
        
            foreach (var card in cards)
            {
                if (card.cardType == CardType.Category)
                {
                    targetSlot.categoryCard = card as CategoryCardActor;
                }
                
                card.currentSlot = targetSlot;
                targetSlot.cards.Push(card);
                card.transform.SetAsLastSibling();
                card.transform.position = targetSlot.transform.position;
                card.FlipCard(0); 
                card.isMoving = false;
                card.canSelected = false;
                
                // Vector3 cardPos = cardGroup.DealCardToBaseSlotOnStart(baseArea.slots[i]);
                // cardGroup.transform.position = cardPos;
                //
                // // 创建进度点
                // if (baseArea.slots[i].allCards.Count > 1)
                // {
                //     if (baseArea.slots[i].allCards.Count < baseArea.slots[i].cardCount + 1)
                //     {
                //         // 创建进度点
                //         if (!baseArea.slots[i].categoryCard.isTextScale)
                //         {
                //             baseArea.slots[i].CreateProgressPoints();
                //         }
                //
                //         // 点亮进度点
                //         if (baseArea.slots[i].progressPointSrs.Count > 0)
                //         {
                //             baseArea.slots[i].LightUpProgressPoint();
                //         }
                //     }
                //     else
                //     {
                //         // 移除进度条
                //         baseArea.slots[i].RemoveProgressPoints();
                //     }
                // }
        
                // 调整分类卡的位置
                if (targetSlot.cards.Count > 1 && !targetSlot.categoryCard.isTextScale)
                {
                    targetSlot.categoryCard.MovingCardTextToSmall(0);
                    targetSlot.categoryCard.MovingCardPos(0);
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
            OperateSlot currentSlot = GameDataUtils.Instance.operateSlots[i % stackSlotCnt];
        
            if (currentSlot.cards.Count < currentSlot.cardInfos.Count)
            {
                // 取出一张卡牌
                CardActor card = GameDataUtils.Instance.cardActors[currentSlot.cardInfos[i / stackSlotCnt]];
                
                // 添加到卡槽中
                currentSlot.AddCard(card);
        
                yield return new WaitForSeconds(DealingInterval);

                // 如果是此卡槽发的最后一张牌，则翻转
                if (currentSlot.cards.Count == currentSlot.cardInfos.Count)
                {
                    currentSlot.CheckFlipCard();
                }
            }
        }
        
        // 将剩余的牌移动到取牌区
        MovingLeftCardsToTakeArea();

        // SetTimerStartOrStop(true);
        
        yield return new WaitForSeconds(0.5f);
        
        EnableScene();
    }
    
    /// <summary>
    /// 将剩余的牌移动到取牌区
    /// </summary>
    private void MovingLeftCardsToTakeArea()
    {
        for (int i = 0; i < levelData.HomeSlot.Infos.Count; i++)
        {
            string cardName = levelData.HomeSlot.Infos[i];
            
            takeGroup.homeSlot.AddCard(GameDataUtils.Instance.cardActors[cardName]);
        }
        
        takeGroup.homeSlot.UpdateCardsPos();
    }

    /// <summary>
    /// 设置高亮边框的层级
    /// </summary>
    private void SetHighlightGroup()
    {
        foreach (var slot in GameDataUtils.Instance.operateSlots)
        {
            slot.highLightBorder.transform.SetParent(highLightGroup.transform);
        }

        foreach (var slot in GameDataUtils.Instance.targetSlots)
        {
            slot.highLightBorder.transform.SetParent(highLightGroup.transform);
        }
    }
    
    /// <summary>
    /// 设置进度点的层级
    /// </summary>
    private void SetProgressPointGroup()
    {
        foreach (var slot in GameDataUtils.Instance.targetSlots)
        {
            slot.progressPointsBar.transform.SetParent(progressPointGroup.transform);
        }
    }

    #region 启用与禁用场景点击响应
    
    /// <summary>
    /// 禁用全部点击响应
    /// </summary>
    private void DisableScene()
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// 启用全部点击响应
    /// </summary>
    private void EnableScene()
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }
    
    #endregion
}
