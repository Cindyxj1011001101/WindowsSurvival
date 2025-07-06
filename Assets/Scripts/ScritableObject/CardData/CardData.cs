using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardData:ScriptableObject
{
    public string cardName;
    public Sprite cardImage;
    public CardType cardType;
    public string cardDesc;
    public int MaxStackNum;
    public List<CardEvent> cardEventList;
    public virtual void Init()
    {
            
    }
}