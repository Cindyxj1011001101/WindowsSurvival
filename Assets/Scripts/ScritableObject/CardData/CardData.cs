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
    public string cardDesc;
    public CardType cardType;
    public float weight;
    public int maxStackNum;
    public int maxEndurance;
    public List<CardEvent> cardEventList;
    public List<CardTag> CardTagList;
    public CardEvent onUsedUp; // 耐久归零触发
}