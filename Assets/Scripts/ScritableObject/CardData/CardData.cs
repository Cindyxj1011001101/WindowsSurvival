using System;
using System.Collections.Generic;
using UnityEngine;

public enum CardTag
{
    Rubbish, // 垃圾
    Cut, // 切割类
    Collectable, // 可采集
}


public enum CardType
{
    Food,
    Tool,
    Resource,
    Place,
    ResourcePoint,
    Equipment,
    Creature,
    Construction,
}


[Serializable]
public class CardData : ScriptableObject
{
    public string cardName;
    public Sprite cardImage;
    [TextArea(3, 5)]
    public string cardDesc;
    public CardType cardType;
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

    private void OnValidate()
    {
        cardName = name;
        Type thisType = GetType();
        if (thisType == typeof(FoodCardData))
        {
            cardType = CardType.Food;
        }
        else if (thisType == typeof(ToolCardData))
        {
            cardType = CardType.Tool;
        }
        else if (thisType == typeof(ResourceCardData))
        {
            cardType = CardType.Resource;
        }
        else if (thisType == typeof(EquipmentCardData))
        {
            cardType = CardType.Equipment;
        }
        else if (thisType == typeof(ResourcePointCardData))
        {
            cardType = CardType.ResourcePoint;
        }
        else if (thisType == typeof(ConstructionCardData))
        {
            cardType = CardType.Construction;
        }
        else if (thisType == typeof(PlaceCardData))
        {
            cardType = CardType.Place;
        }
    }
}