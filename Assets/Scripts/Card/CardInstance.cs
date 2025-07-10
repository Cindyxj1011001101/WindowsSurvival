using System;
using UnityEngine;

public abstract class CardInstance : IComparable<CardInstance>
{
    public string dataPath;

    public int currentEndurance;

    private CardSlot slot;

    public CardData GetCardData() => Resources.Load<CardData>(dataPath);

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
        return GetCardData().ToString();
    }

    public void Use()
    {
        // maxEndurance <= 0表示无限耐久
        if (GetCardData().maxEndurance <= 0) return;

        currentEndurance--;
        if (currentEndurance <= 0)
        {
            // 销毁这张卡牌
            DestroyThisCard();
            // 触发卡牌耐久归零事件
            EffectResolve.Instance.Resolve(GetCardData().onUsedUp);
        }
    }

    protected virtual void DestroyThisCard()
    {
        slot.Bag.RemoveCard(this);
    }
}