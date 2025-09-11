using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SlotCards : IComparable<SlotCards>
{
    public List<Card> Cards { get; protected set; } = new();

    [JsonProperty] private int maxStackNum = int.MaxValue;

    // 序列化这个会造成循环引用
    [JsonIgnore] public Bag Bag { get; protected set; }

    [JsonIgnore] public bool IsEmpty => Cards.IsNullOrEmpty();

    [JsonIgnore] public int StackNum => Cards.Count;

    [JsonIgnore] public Card this[int index] => Cards[index];

    [JsonIgnore] public CardSlot CardSlot { get; protected set; }

    public void Init()
    {
        foreach (var card in Cards)
        {
            card.SetSlotCards(this);
            card.StartUpdating();
        }
    }

    public void SetMaxStackNum(int maxStackNum)
    {
        this.maxStackNum = maxStackNum;
    }

    public void SetCardSlot(CardSlot slot)
    {
        CardSlot = slot;
    }

    public void SetBag(Bag bag)
    {
        Bag = bag;
    }

    public virtual void AddCard(Card card)
    {
        // 记录卡牌原来的背包
        var oBag = card.Bag;

        Cards.Add(card);
        Cards.Sort((a, b) => a.CompareTo(b));

        card.SetSlotCards(this);

        Bag.OnAddCard(card);
        
        // 如果卡牌从不同的背包添加而来
        if (oBag != Bag)
        {
            // oBag为空说明卡牌是第一次创建
            if (oBag != null) card.OnRemoved(oBag); // 不必担心卡牌被销毁时执行不到这里的onremoved方法，它会转而在RemoveCard中执行
            card.OnAdded(Bag);
        }
    }

    /// <summary>
    /// 移除指定的一张卡牌
    /// </summary>
    /// <param name="card"></param>
    public virtual void RemoveCard(Card card)
    {
        if (!Cards.Contains(card)) return;

        Cards.Remove(card);

        Bag.OnRemoveCard(card);

        // 如果卡牌要被销毁，说明它不会进入AddCard方法，需要在这里执行onremoved
        if (card.Destroyed) card.OnRemoved(Bag);

        card.RefreshSlot();
    }

    /// <summary>
    /// 移除最优先显示的卡牌
    /// </summary>
    /// <returns></returns>
    public Card RemoveCard()
    {
        var cardToRemove = PeekCard();

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

    /// <summary>
    /// 销毁最优先的卡牌
    /// </summary>
    public void DestroyCard()
    {
        PeekCard().DestroyThis();
    }

    /// <summary>
    /// 销毁指定数量的卡牌
    /// </summary>
    public void DestroyCards(int amount)
    {
        for (int i = 0; i < amount; i++)
            DestroyCard();
    }

    /// <summary>
    /// 最优先的卡牌
    /// </summary>
    /// <returns></returns>
    public Card PeekCard() => Cards[0];

    /// <summary>
    /// 判断该卡牌格是否放有同类卡牌（名称相同即同类）
    /// </summary>
    /// <param name="cardName"></param>
    /// <returns></returns>
    public bool ContainsByCardName(string cardName) => !IsEmpty && cardName == PeekCard().CardName;

    /// <summary>
    /// 判断该卡牌格是否放有相同卡牌（ID相同）
    /// </summary>
    /// <param name="cardId"></param>
    /// <returns></returns>
    public bool ContainsByCardId(string cardId) => !IsEmpty && cardId == PeekCard().CardId;

    /// <summary>
    /// 能否添加指定卡牌，只有id相同才能堆叠
    /// </summary>
    /// <returns></returns>
    public virtual bool CanAddCard(Card card)
    {
        return IsEmpty || (card.CardId == PeekCard().CardId && StackNum < Mathf.Min(card.MaxStackNum, maxStackNum));
    }

    /// <summary>
    /// 销毁所有卡牌
    /// </summary>
    public virtual void Clear()
    {
        DestroyCards(StackNum);
        Cards.Clear();
    }

    public int CompareTo(SlotCards other)
    {
        if (IsEmpty) return 1; // 自己是空，则自己排在后面

        if (other.IsEmpty) return -1; // other是空，自己排在前面

        // 都不为空
        var thisCard = PeekCard();
        var otherCard = other.PeekCard();

        // 渗水裂缝特殊处理，保证渗水裂缝永远显示在最前面
        if (thisCard.CardId == "渗水裂缝") return -1;
        if (otherCard.CardId == "渗水裂缝") return 1;

        // 类型不同，按照类型排序
        if (thisCard.CardType != otherCard.CardType) return thisCard.CardType - otherCard.CardType;

        // 类型相同，卡牌id不同，按id排序
        if (thisCard.CardId != otherCard.CardId) return string.Compare(thisCard.CardId, otherCard.CardId);

        // 卡牌id相同，按堆叠数量排序
        return this.StackNum - other.StackNum;
    }
}