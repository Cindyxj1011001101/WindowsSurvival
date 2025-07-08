using System;
using System.Collections.Generic;
using UnityEngine;

public enum CardTag
{
    Rubbish,
}

[Serializable]
public class CardData:ScriptableObject
{
    public string cardName;
    public Sprite cardImage;
    public CardType cardType;
    public string cardDesc;
    public float Weight;
    public int maxStackNum;
    public List<CardEvent> cardEventList;
    public List<CardTag> CardTagList;
    public virtual void Init()
    {
            
    }
}