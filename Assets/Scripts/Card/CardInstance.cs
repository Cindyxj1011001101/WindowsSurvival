using System;
using UnityEngine;

public abstract class CardInstance : IComparable<CardInstance>
{
    public string dataPath;

    public CardData CardData => Resources.Load<CardData>(dataPath);

    public virtual int CompareTo(CardInstance other)
    {
        return 0;
    }

    public abstract void InitFromCardData(CardData cardData);

    public override string ToString()
    {
        return CardData.ToString();
    }
}