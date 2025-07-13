using System;
using System.Collections.Generic;
using UnityEngine;

public enum CardTag
{
    Rubbish,
    Cut,
}

[Serializable]
public class CardData : ScriptableObject
{
    public string cardName;
    public Sprite cardImage;
    public CardType cardType;
    public string cardDesc;
    public float weight;
    public int maxStackNum;
    public int maxEndurance;
    public List<CardEvent> cardEventList;
    public List<CardTag> CardTagList;
    public CardEvent onUsedUp; // 耐久归零触发

    public override bool Equals(object other)
    {
        if (other is not CardData) return false;
        return (other as CardData).cardName == cardName;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}