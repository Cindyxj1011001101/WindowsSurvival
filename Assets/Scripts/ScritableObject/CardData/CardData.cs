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
    public int MaxStackNum;
    public List<CardEvent> cardEventList;
    public List<CardTag> CardTagList;
    public virtual void Init()
    {
            
    }
}