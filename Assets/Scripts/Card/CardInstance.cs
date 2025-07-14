using Newtonsoft.Json;
using System;
using UnityEngine;

public abstract class CardInstance : IComparable<CardInstance>
{
    public string dataPath;

    public int currentEndurance;

    private CardSlot slot;

    [JsonIgnore]
    public CardSlot Slot => slot;

    [JsonIgnore]
    public CardData CardData => Resources.Load<CardData>(dataPath);

    public void SetCardSlot(CardSlot slot)
    {
        this.slot = slot;
    }

    public virtual int CompareTo(CardInstance other)
    {
        return 0;
    }

    public virtual void InitFromCardData(CardData cardData)
    {
        currentEndurance = cardData.maxEndurance;
    }

    public override string ToString()
    {
        return CardData.ToString();
    }

    public void Use()
    {
        // maxEndurance <= 0表示无限耐久
        if (CardData.maxEndurance <= 0) return;

        currentEndurance--;
        if (currentEndurance <= 0)
        {
            // 销毁这张卡牌
            DestroyThisCard();
            // 触发卡牌耐久归零事件
            GameManager.Instance.HandleCardEvent(CardData.onUsedUp);
        }
        // 刷新前端显示的卡牌数据
        EventManager.Instance.TriggerEvent(EventType.ChangeCardProperty);
    }

    protected virtual void DestroyThisCard()
    {
        //slot.Bag.RemoveCard(slot);
        slot.RemoveCard(this);
    }
}