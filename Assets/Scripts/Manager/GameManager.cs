using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

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

    public Stack<Card> currentPickedCard; //当前拿着的卡牌
    public CardGroup currentPickedCardGroup; // 当前拿着的牌组

    public List<Card> testList; // 测试用

    private void Start()
    {
        currentPickedCard = new Stack<Card>();
        testList = new List<Card>();
    }

    private void Update()
    {
        testList = currentPickedCard.ToList();
    }
}
