using System;
using UnityEngine;

public abstract class CardInstance : IComparable<CardInstance>
{
    public string dataPath;

    public CardData CardData => Resources.Load<CardData>(dataPath);

    public abstract int CompareTo(CardInstance other);

    public abstract void InitFromCardData(CardData cardData);

    public override string ToString()
    {
        return CardData.ToString();
    }
}