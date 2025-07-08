
using UnityEngine;

[CreateAssetMenu(fileName = "FoodCardData", menuName = "ScritableObject/FoodCardData")]
public class FoodCardData:CardData
{
    public int MaxFresh;

    public override void Init()
    { 
        cardType=CardType.Food;
    }
}