using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ���Ƹ�
/// </summary>
public class CardSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text countText; // ������ʾ�������;õ�
    [SerializeField] private Text percentageText; // ������ʾ���ʶȻ��;�
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
    /// ˢ�µ�ǰ��ʾ
    /// </summary>
    public void RefreshCurrentDisplay()
    {
        if (IsEmpty) return;

        DisplayCard(PeekCard(), StackCount);
    }

    /// <summary>
    /// ��ʾָ�������Ŀ���
    /// </summary>
    /// <param name="card"></param>
    /// <param name="stackCount"></param>
    public void DisplayCard(Card card, int stackCount)
    {
        // ���Ҫ��ʾ������С�ڵ����㣬��ʲôҲ����ʾ
        if (stackCount <= 0)
        {
            DisableDisplay();
            return;
        }

        EnableDisplay();

        iconImage.sprite = card.CardImage;
        nameText.text = card.cardName;
        // ��ʾ�ѵ�����
        countText.text = "";
        if (stackCount > 1)
            countText.text = $"x{stackCount}";

        percentageText.text = "";

        // ��ʾ�;�
        if (card.TryGetComponent<DurabilityComponent>(out var durabilityComponent))
            percentageText.text = $"{Math.Round((float)durabilityComponent.durability / durabilityComponent.maxDurability, 2) * 100}%";
        // ��ʾ���ʶ�
        else if (card.TryGetComponent<FreshnessComponent>(out var component))
            percentageText.text = $"{Math.Round((float)component.freshness / component.maxFreshness, 2) * 100}%";

        // ��ʾ������

        // ��ʾ�������

    }

    /// <summary>
    /// ����ʾ����
    /// </summary>
    private void DisableDisplay()
    {
        cardCanvasGroup.alpha = 0;
        cardCanvasGroup.blocksRaycasts = false;
        cardCanvasGroup.interactable = false;
    }

    /// <summary>
    /// ������ʾ����
    /// </summary>
    private void EnableDisplay()
    {
        cardCanvasGroup.alpha = 1;
        cardCanvasGroup.blocksRaycasts = true;
        cardCanvasGroup.interactable = true;
    }

    public bool ContainsSimilarCard(string cardName) => !IsEmpty && cardName == cards[0].cardName;
    
    /// <summary>
    /// �ܷ�ѵ�����ʹ�ø÷���ǰ�����ȷ��Ҫ�ѵ��Ŀ��ƺ����slot���еĿ�����ͬ���
    /// </summary>
    /// <returns></returns>
    public virtual bool CanAddCard(Card card)
    {
        return IsEmpty || (ContainsSimilarCard(card.cardName) && StackCount < card.maxStackNum);
    }

    /// <summary>
    /// ���һ�ſ���
    /// </summary>
    /// <param name="card"></param>
    public virtual void AddCard(Card card)
    {
        cards.Add(card);
        cards.Sort((a, b) => a.CompareTo(b));

        RefreshCurrentDisplay();

        // ��������ӵ���ұ���ʱ
        if (bag is PlayerBag)
            EventManager.Instance.TriggerEvent(EventType.ChangePlayerBagCards,
                new ChangePlayerBagCardsArgs { card = card, add = 1 });
        //// ��װ������ʱ
        //if (bag is EquipmentBag)
        //    EventManager.Instance.TriggerEvent(EventType.Equip, card);

        card.SetCardSlot(this);
    }

    /// <summary>
    /// �Ƴ�ָ����һ�ſ���
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

        // �����ƴ���ұ����Ƴ�ʱ
        if (bag is PlayerBag)
            EventManager.Instance.TriggerEvent(EventType.ChangePlayerBagCards,
                new ChangePlayerBagCardsArgs { card = card, add = -1 });
        //// ��ж��װ��ʱ
        //if (bag is EquipmentBag)
        //    EventManager.Instance.TriggerEvent(EventType.Unequip, card);
    }

    /// <summary>
    /// �Ƴ���������ʾ�Ŀ���
    /// </summary>
    /// <returns></returns>
    public Card RemoveCard()
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