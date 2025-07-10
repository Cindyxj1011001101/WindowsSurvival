
using UnityEngine;

[CreateAssetMenu(fileName = "FoodCardData", menuName = "ScritableObject/FoodCardData")]
public class FoodCardData:CardData
{
    public int MaxFresh;

    public CardEvent onRotton; // 当食物腐烂时触发
}