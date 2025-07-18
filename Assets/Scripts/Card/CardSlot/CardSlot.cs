using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 卡牌格
/// </summary>
public class CardSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text countText; // 用于显示数量和耐久等
    [SerializeField] private Text percentageText; // 用于显示新鲜度或耐久
    [SerializeField] private Text nameText;
    [SerializeField] private CanvasGroup cardCanvasGroup;

    private List<Card> cards = new();
    public bool IsEmpty => cards.Count == 0;
    public int StackCount => cards.Count;

    public List<Card> Cards => cards;

    private BagBase bag;
    public BagBase Bag => bag;

    private void Awake()
    {
        if (cardCanvasGroup.TryGetComponent<DoubleClickHandler>(out var doubleClickHandler))
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
        foreach (var card in cardSlotRuntimeData.cardList)
        {
            AddCard(card);
            card.StartUpdating();
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
    /// 显示指定数量的卡牌
    /// </summary>
    /// <param name="card"></param>
    /// <param name="stackCount"></param>
    public void DisplayCard(Card card, int stackCount)
    {
        // 如果要显示的数量小于等于零，则什么也不显示
        if (stackCount <= 0)
        {
            DisableDisplay();
            return;
        }

        EnableDisplay();

        iconImage.sprite = card.CardImage;
        nameText.text = card.cardName;
        // 显示堆叠数量
        countText.text = "";
        if (stackCount > 1)
            countText.text = $"x{stackCount}";

        percentageText.text = "";

        // 显示耐久
        if (card.TryGetComponent<DurabilityComponent>(out var durabilityComponent))
            percentageText.text = $"{Math.Round((float)durabilityComponent.durability / durabilityComponent.maxDurability, 2) * 100}%";
        // 显示新鲜度
        else if (card.TryGetComponent<FreshnessComponent>(out var component))
            percentageText.text = $"{Math.Round((float)component.freshness / component.maxFreshness, 2) * 100}%";

        // 显示生长度

        // 显示产物进度

    }

    /// <summary>
    /// 不显示卡牌
    /// </summary>
    private void DisableDisplay()
    {
        cardCanvasGroup.alpha = 0;
        cardCanvasGroup.blocksRaycasts = false;
        cardCanvasGroup.interactable = false;
    }

    /// <summary>
    /// 允许显示卡牌
    /// </summary>
    private void EnableDisplay()
    {
        cardCanvasGroup.alpha = 1;
        cardCanvasGroup.blocksRaycasts = true;
        cardCanvasGroup.interactable = true;
    }

    public bool ContainsSimilarCard(string cardName) => !IsEmpty && cardName == cards[0].cardName;
    
    /// <summary>
    /// 能否堆叠，在使用该方法前请务必确认要堆叠的卡牌和这个slot放有的卡牌是同类的
    /// </summary>
    /// <returns></returns>
    public virtual bool CanAddCard(Card card)
    {
        return IsEmpty || (ContainsSimilarCard(card.cardName) && StackCount < card.maxStackNum);
    }

    /// <summary>
    /// 添加一张卡牌
    /// </summary>
    /// <param name="card"></param>
    public virtual void AddCard(Card card)
    {
        cards.Add(card);
        cards.Sort((a, b) => a.CompareTo(b));

        RefreshCurrentDisplay();

        // 当卡牌添加到玩家背包时
        if (bag is PlayerBag)
            EventManager.Instance.TriggerEvent(EventType.ChangePlayerBagCards,
                new ChangePlayerBagCardsArgs { card = card, add = 1 });
        //// 当装备卡牌时
        //if (bag is EquipmentBag)
        //    EventManager.Instance.TriggerEvent(EventType.Equip, card);

        card.SetCardSlot(this);
    }

    /// <summary>
    /// 移除指定的一张卡牌
    /// </summary>
    /// <param name="card"></param>
    public virtual void RemoveCard(Card card)
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
        //// 当卸下装备时
        //if (bag is EquipmentBag)
        //    EventManager.Instance.TriggerEvent(EventType.Unequip, card);
    }

    /// <summary>
    /// 移除最优先显示的卡牌
    /// </summary>
    /// <returns></returns>
    public Card RemoveCard()
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

    public Card PeekCard() => cards[0];

    public void ClearSlot()
    {
        cards.Clear();
        DisableDisplay();
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EventType.ChangeCardProperty, RefreshCurrentDisplay);
    }
}