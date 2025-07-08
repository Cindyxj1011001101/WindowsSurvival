using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
[Serializable]
public abstract class Bag
{
    public List<Card> cardList = new List<Card>();
    public abstract void Init();
    public abstract void AddCard(CardData cardData);
    public abstract void RemoveCard(Card card);
}

