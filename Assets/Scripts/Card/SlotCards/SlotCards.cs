using Newtonsoft.Json;
using System.Collections.Generic;

public class SlotCards
{
    [JsonIgnore] // 序列化这个会造成循环引用
    public Bag Bag { get; protected set; }

    public List<Card> Cards { get; protected set; } = new();

    [JsonIgnore]
    public bool IsEmpty => Cards.IsNullOrEmpty();

    [JsonIgnore]
    public int StackNum => Cards.Count;

    [JsonIgnore]
    public Card this[int index] => Cards[index];

    [JsonIgnore]
    public CardSlot CardSlot { get; protected set; }

    public void Init()
    {
        foreach (var card in Cards)
        {
            card.SetSlotCards(this);
            card.StartUpdating();
        }
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
        Cards.Add(card);
        Cards.Sort((a, b) => a.CompareTo(b));

        card.SetSlotCards(this);
     
        card.StartUpdating();

        Bag.OnAddCard(card);
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

        if (CardSlot != null) CardSlot.RefreshDisplay();
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
        return IsEmpty || (card.CardId == PeekCard().CardId && StackNum < card.MaxStackNum);
    }

    public virtual void Clear()
    {
        while (!IsEmpty)
        {
            RemoveCard();
        }
        Cards.Clear();
    }
}