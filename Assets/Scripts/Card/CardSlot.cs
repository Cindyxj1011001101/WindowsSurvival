using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image fillImage; 
    [SerializeField] private Text propertyText;
    [SerializeField] private Text nameText;
    [SerializeField] private Transform cardTransform;

    private CardData currentCard;

    private List<CardInstance> cards = new();

    public bool IsEmpty => card == null;
    public CardData CardData => currentCard;
    public Card card;

    public int StackCount => cards.Count;

    public List<CardInstance> Cards => cards;

    private BagBase bag;
    public BagBase Bag => bag;

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
        if (cardTransform.TryGetComponent<DoubleClickHandler>(out var doubleClickHandler))
        {
            doubleClickHandler.onDoubleClick.AddListener(() =>
            {
                (WindowsManager.Instance.OpenWindow("Details") as DetailsWindow).Refresh(this);
            });
        }

        EventManager.Instance.AddListener(EventType.ChangeCardProperty, RefreshCurrentDisplay);
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

    private void RefreshCurrentDisplay()
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
                propertyText.text = $"{Math.Round((float)(card as ResourcePointCardInstance).currentEndurance / cardData.maxEndurance, 1) * 100} %";
                break;

            case ToolCardData cardData:
                propertyText.text = $"{Math.Round((float)(card as ToolCardInstance).currentEndurance / cardData.maxEndurance, 1) * 100} %";
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

    /// <summary>
    /// ����һ�ſ���
    /// </summary>
    /// <param name="card"></param>
    public void AddCard(CardInstance card)
    {
        currentCard = card.CardData;

        cards.Add(card);
        cards.Sort((a, b) => a.CompareTo(b));

        RefreshCurrentDisplay();

        // ���������ӵ���ұ���ʱ
        if (bag is PlayerBag)
            EventManager.Instance.TriggerEvent(EventType.ChangePlayerBagCards,
                new ChangePlayerBagCardsArgs { card = card, add = 1 });
        // ��װ������ʱ
        if (bag is EquipmentBag)
            EventManager.Instance.TriggerEvent(EventType.Equip, card as EquipmentCardInstance);

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
            RefreshCurrentDisplay();

        // �����ƴ���ұ����Ƴ�ʱ
        if (bag is PlayerBag)
            EventManager.Instance.TriggerEvent(EventType.ChangePlayerBagCards,
                new ChangePlayerBagCardsArgs { card = card, add = -1 });
        // ��ж��װ��ʱ
        if (bag is EquipmentBag)
            EventManager.Instance.TriggerEvent(EventType.Unequip, card as EquipmentCardInstance);
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

    /// <summary>
    /// �Ƴ�ָ�������Ŀ���
    /// </summary>
    /// <param name="amount"></param>
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
        EventManager.Instance.RemoveListener(EventType.ChangeCardProperty, RefreshCurrentDisplay);
    }
}