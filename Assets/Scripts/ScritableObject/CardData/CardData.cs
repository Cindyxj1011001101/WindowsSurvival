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
    public int maxStackNum;
    public List<CardEvent> cardEventList;
    public virtual void Init()
    {
            
    }

    public override string ToString()
    {
        return $"cardName: {cardName}, cardType: {cardType}, cardDesc: {cardDesc}, maxStackNum: {maxStackNum}, cardEventList: {cardEventList}";
    }
}