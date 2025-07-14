using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image fillImage; // ������ʾ���ʶȵ�
    [SerializeField] private Text propertyText; // ������ʾ�������;õ�
    [SerializeField] private Text nameText;
    [SerializeField] private Transform cardTransform;

    private CardData currentCard;

    private List<CardInstance> cards = new();

    public bool IsEmpty => currentCard == null;
    public CardData CardData => currentCard;
    public int StackCount => cards.Count;

    public List<CardInstance> Cards => cards;

    private BagBase bag;
    public BagBase Bag => bag;

    /// <summary>
    /// �ܷ�米���ƶ�
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
        //iconImage = transform.Find("Card/Icon").GetComponent<Image>();
        //fillImage = transform.Find("Card/Fill").GetComponent<Image>();
        //propertyText = transform.Find("Card/Property").GetComponent<Text>();
        //nameText = transform.Find("Card/Name").GetComponent<Text>();
        //cardTransform = transform.Find("Card");
        if (cardTransform.TryGetComponent<DoubleClickHandler>(out var doubleClickHandler))
        {
            doubleClickHandler.onDoubleClick.AddListener(() =>
            {
                (WindowsManager.Instance.OpenWindow("Details") as DetailsWindow).Refresh(this);
            });
        }

        EventManager.Instance.AddListener(EventType.ChangeCardProperty, OnCardPropertyChanged);
    }

    public void SetBag(BagBase bag)
    {
        this.bag = bag;
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

        DisplayCard(PeekCard());
    }

    public void DisplayCard(CardInstance card)
    {
        cardTransform.gameObject.SetActive(true);
        var data = card.CardData;
        iconImage.sprite = data.cardImage;
        nameText.text = data.cardName;
        fillImage.gameObject.SetActive(data is FoodCardData);
        propertyText.text = "";
        switch (data)
        {
            case FoodCardData cardData:
                // ����������
                if (cardData.MaxFresh == -1)
                    fillImage.gameObject.SetActive(false);
                // �б�����
                else
                    fillImage.fillAmount = (float)(card as FoodCardInstance).currentFresh / cardData.MaxFresh;

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
                propertyText.text = $"{Math.Round((float)(card as ResourcePointCardInstance).currentEndurance / cardData.maxEndurance, 1)} %";
                break;

            case ToolCardData cardData:
                propertyText.text = $"{Math.Round((float)(card as ToolCardInstance).currentEndurance / cardData.maxEndurance, 1)} %";
                break;

            default:
                break;
        }
    }

    public bool ContainsSimilarCard(CardData cardData) => !IsEmpty && currentCard.Equals(cardData);
    
    /// <summary>
    /// �ܷ�ѵ�����ʹ�ø÷���ǰ�����ȷ��Ҫ�ѵ��Ŀ��ƺ����slot���еĿ�����ͬ���
    /// </summary>
    /// <returns></returns>
    public bool CanStack() => StackCount < currentCard.maxStackNum;

    public void AddCard(CardInstance card)
    {
        currentCard = card.CardData;

        cards.Add(card);
        cards.Sort((a, b) => a.CompareTo(b));

        OnCardPropertyChanged();

        if (bag is PlayerBag && (card.Slot == null || card.Slot.bag is not PlayerBag))
            //bag.OnCardAdded(card);
            EventManager.Instance.TriggerEvent(EventType.ChangePlayerBagCards,
                new ChangePlayerBagCardsArgs { card = card, add = 1 });

        card.SetCardSlot(this);
    }

    /// <summary>
    /// �Ƴ�ָ����һ�ſ���
    /// </summary>
    /// <param name="card"></param>
    public void RemoveCard(CardInstance card)
    {
        if (!cards.Contains(card)) return;

        cards.Remove(card);
        card.SetCardSlot(null);

        if (StackCount == 0)
            ClearSlot();
        else
            OnCardPropertyChanged();


        if (bag is PlayerBag && (card.Slot == null || card.Slot.bag is not PlayerBag))
            //bag.OnCardAdded(card);
            EventManager.Instance.TriggerEvent(EventType.ChangePlayerBagCards,
                new ChangePlayerBagCardsArgs { card = card, add = -1 });
    }

    /// <summary>
    /// �Ƴ���������ʾ�Ŀ���
    /// </summary>
    /// <returns></returns>
    public CardInstance RemoveCard()
    {
        var cardToRemove = cards[0];

        RemoveCard(cardToRemove);

        return cardToRemove;
    }

    public void RemoveCards(int amount)
    {
        for (int i = 0; i < amount; i++)
            RemoveCard();
    }

    /// <summary>
    /// �Ƴ����п���
    /// </summary>
    public void RemoveAllCards()
    {
        while (StackCount > 0)
        {
            RemoveCard();
        }
    }

    public CardInstance PeekCard() => cards[0];

    public void ClearSlot()
    {
        currentCard = null;
        RemoveAllCards();
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