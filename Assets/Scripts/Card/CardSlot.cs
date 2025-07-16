using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image fillImage; // 用于显示新鲜度等
    [SerializeField] private Text propertyText; // 用于显示数量和耐久等
    [SerializeField] private Text nameText;
    [SerializeField] private CanvasGroup cardCanvas;

    private CardData currentCard;

    private List<CardInstance> cards = new();

    public bool IsEmpty => currentCard == null;
    public CardData CardData => currentCard;
    public int StackCount => cards.Count;

    public List<CardInstance> Cards => cards;

    private BagBase bag;
    public BagBase Bag => bag;

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
        if (cardCanvas.TryGetComponent<DoubleClickHandler>(out var doubleClickHandler))
        {
            doubleClickHandler.onDoubleClick.AddListener(() =>
            {
                (WindowsManager.Instance.OpenWindow("Details") as DetailsWindow).Refresh(this);
            });
        }

        EventManager.Instance.AddListener<CardSlot>(EventType.ChangeCardProperty, RefreshCurrentDisplay);
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

    /// <summary>
    /// 刷新当前显示
    /// </summary>
    public void RefreshCurrentDisplay()
    {
        if (IsEmpty) return;

        DisplayCard(PeekCard(), StackCount);
    }

    /// <summary>
    /// 刷新当前显示，只有当发生属性变化的卡牌的属于当前slot时才执行
    /// </summary>
    /// <param name="slot"></param>
    private void RefreshCurrentDisplay(CardSlot slot)
    {
        if (slot != this) return;
        RefreshCurrentDisplay();
    }

    /// <summary>
    /// 显示指定数量的卡牌
    /// </summary>
    /// <param name="card"></param>
    /// <param name="stackCount"></param>
    public void DisplayCard(CardInstance card, int stackCount)
    {
        // 如果要显示的数量小于等于零，则什么也不显示
        if (stackCount <= 0)
        {
            DisableDisplay();
            return;
        }

        EnableDisplay();
        var data = card.CardData;
        iconImage.sprite = data.cardImage;
        nameText.text = data.cardName;
        fillImage.gameObject.SetActive(data is FoodCardData);
        propertyText.text = "";
        switch (data)
        {
            case FoodCardData cardData:
                // 保质期无限
                if (cardData.MaxFresh == -1)
                    fillImage.gameObject.SetActive(false);
                // 有保质期
                else
                    fillImage.fillAmount = (float)(card as FoodCardInstance).currentFresh / cardData.MaxFresh;

                if (stackCount > 1)
                    propertyText.text = $"x{stackCount}";

                break;

            case ResourceCardData:
                if (stackCount > 1)
                    propertyText.text = $"x{stackCount}";
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

    /// <summary>
    /// 不显示卡牌
    /// </summary>
    private void DisableDisplay()
    {
        //iconImage.sprite = null;
        //nameText.text = "";
        //propertyText.text = "";
        cardCanvas.alpha = 0;
        cardCanvas.blocksRaycasts = false;
        cardCanvas.interactable = false;
    }

    /// <summary>
    /// 允许显示卡牌
    /// </summary>
    private void EnableDisplay()
    {
        cardCanvas.alpha = 1;
        cardCanvas.blocksRaycasts = true;
        cardCanvas.interactable = true;
    }

    public bool ContainsSimilarCard(CardData cardData) => !IsEmpty && currentCard.Equals(cardData);
    
    /// <summary>
    /// 能否堆叠，在使用该方法前请务必确认要堆叠的卡牌和这个slot放有的卡牌是同类的
    /// </summary>
    /// <returns></returns>
    public bool CanAddCard(CardInstance card)
    {
        return IsEmpty || (ContainsSimilarCard(card.CardData) && StackCount < currentCard.maxStackNum);
    }

    /// <summary>
    /// 添加一张卡牌
    /// </summary>
    /// <param name="card"></param>
    public void AddCard(CardInstance card)
    {
        currentCard = card.CardData;

        cards.Add(card);
        cards.Sort((a, b) => a.CompareTo(b));

        RefreshCurrentDisplay();

        // 当卡牌添加到玩家背包时
        if (bag is PlayerBag)
            EventManager.Instance.TriggerEvent(EventType.ChangePlayerBagCards,
                new ChangePlayerBagCardsArgs { card = card, add = 1 });
        // 当装备卡牌时
        if (bag is EquipmentBag)
            EventManager.Instance.TriggerEvent(EventType.Equip, card as EquipmentCardInstance);

        card.SetCardSlot(this);
    }

    /// <summary>
    /// 移除指定的一张卡牌
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

        // 当卡牌从玩家背包移除时
        if (bag is PlayerBag)
            EventManager.Instance.TriggerEvent(EventType.ChangePlayerBagCards,
                new ChangePlayerBagCardsArgs { card = card, add = -1 });
        // 当卸下装备时
        if (bag is EquipmentBag)
            EventManager.Instance.TriggerEvent(EventType.Unequip, card as EquipmentCardInstance);
    }

    /// <summary>
    /// 移除最优先显示的卡牌
    /// </summary>
    /// <returns></returns>
    public CardInstance RemoveCard()
    {
        var cardToRemove = cards[0];

        RemoveCard(cardToRemove);

        return cardToRemove;
    }

    /// <summary>
    /// 移除指定数量的卡牌
    /// </summary>
    /// <param name="amount"></param>
    public void RemoveCards(int amount)
    {
        for (int i = 0; i < amount; i++)
            RemoveCard();
    }

    ///// <summary>
    ///// 移除所有卡牌
    ///// </summary>
    //public void RemoveAllCards()
    //{
    //    while (StackCount > 0)
    //    {
    //        RemoveCard();
    //    }
    //}

    public CardInstance PeekCard() => cards[0];

    public void ClearSlot()
    {
        currentCard = null;
        //RemoveAllCards();
        cards.Clear();
        DisableDisplay();
    }


    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<CardSlot>(EventType.ChangeCardProperty, RefreshCurrentDisplay);
    }
}