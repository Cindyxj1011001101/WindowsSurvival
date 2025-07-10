using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardSlot : MonoBehaviour
{
    private Image iconImage;
    private Image fillImage; // 用于显示新鲜度等
    private Text propertyText; // 用于显示数量和耐久等
    private Text nameText;
    private Transform cardTransform;

    private CardData currentCard;

    //private PriorityQueue<CardInstance> cardInstanceQueue = new();
    private List<CardInstance> cards = new();

    public bool IsEmpty => currentCard == null;
    public CardData CardData => currentCard;
    //public int StackCount => cardInstanceQueue.Count;
    public int StackCount => cards.Count;

    public List<CardInstance> Cards => cards;

    /// <summary>
    /// 能否跨背包移动
    /// </summary>
    public bool CanDragOverBag
    {
        get
        {
            if (currentCard == null) return false;
            return currentCard is FoodCardData || currentCard is ResourceCardData || currentCard is ToolCardData;
        }
    }

    private void Awake()
    {
        iconImage = transform.Find("Card/Icon").GetComponent<Image>();
        fillImage = transform.Find("Card/Fill").GetComponent<Image>();
        propertyText = transform.Find("Card/Property").GetComponent<Text>();
        nameText = transform.Find("Card/Name").GetComponent<Text>();
        cardTransform = transform.Find("Card");

        cardTransform.GetComponent<DoubleClickHandler>().onDoubleClick.AddListener(() =>
        {
            (WindowsManager.Instance.OpenWindow("Details") as DetailsWindow).Refresh(this);
        });

        //ClearSlot();
    }

    private void Start()
    {
        EventManager.Instance.AddListener(EventType.ChangeCardProperty, OnCardPropertyChanged);
    }

    public void InitFromRuntimeData(CardSlotRuntimeData cardSlotRuntimeData)
    {
        foreach (var card in cardSlotRuntimeData.cardInstanceList)
        {
            AddCard(card);
        }
    }

    private void OnCardPropertyChanged()
    {
        if (IsEmpty) return;

        fillImage.gameObject.SetActive(currentCard is FoodCardData);
        propertyText.text = "";
        switch (currentCard)
        {
            case FoodCardData cardData:
                // 保质期无限
                if (cardData.MaxFresh == -1)
                    fillImage.gameObject.SetActive(false);
                // 有保质期
                else
                    fillImage.fillAmount = (float)(PeekCard() as FoodCardInstance).currentFresh / cardData.MaxFresh;

                if (StackCount > 1)
                    propertyText.text = $"x{StackCount}";

                break;

            case ResourceCardData:
                if (StackCount > 1)
                    propertyText.text = $"x{StackCount}";
                break;

            case PlaceCardData:
                break;

            case ResourcePointCardData cardData:
                propertyText.text = $"{(PeekCard() as ResourcePointCardInstance).currentEndurance} / {cardData.maxEndurance}";
                break;

            case ToolCardData cardData:
                propertyText.text = $"{(PeekCard() as ToolCardInstance).currentEndurance} / {cardData.maxEndurance}";
                break;

            default:
                break;
        }
    }

    public bool ContainsSimilarCard(string cardName) => !IsEmpty && currentCard.cardName == cardName;
    
    /// <summary>
    /// 能否堆叠，在使用该方法前请务必确认要堆叠的卡牌和这个slot放有的卡牌是同类的
    /// </summary>
    /// <returns></returns>
    public bool CanStack() => StackCount < currentCard.maxStackNum;

    public void AddCard(CardInstance card)
    {
        if (IsEmpty)
        {
            cardTransform.gameObject.SetActive(true);
            currentCard = card.GetCardData();
            iconImage.sprite = currentCard.cardImage;
            nameText.text = currentCard.cardName;
        }

        //cardInstanceQueue.Enqueue(card);
        cards.Add(card);
        cards.Sort((a, b) => a.CompareTo(b));

        OnCardPropertyChanged();
    }

    public CardInstance RemoveCard()
    {
        //var cardToRemove = cardInstanceQueue.Dequeue();
        var cardToRemove = cards[0];
        cards.RemoveAt(0);

        if (StackCount == 0)
            ClearSlot();
        else
            OnCardPropertyChanged();

        return cardToRemove;
    }

    //public CardInstance PeekCard() => cardInstanceQueue.Peek();
    public CardInstance PeekCard() => cards[0];

    public void ClearSlot()
    {
        currentCard = null;
        //cardInstanceQueue.Clear();
        cards.Clear();
        iconImage.sprite = null;
        nameText.text = "";
        propertyText.text = "";
        cardTransform.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EventType.ChangeCardProperty, OnCardPropertyChanged);
    }
}