using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 制作配方类型
/// </summary>
public enum RecipeType
{
    Food = 0,
    Oxygen = 1,
    Medic = 2,
    Tool = 3,
    Material = 4,
}

[Serializable]
public class RecipeMaterial
{
    public CardData cardData;
    public int requiredAmount;
}


[CreateAssetMenu(fileName = "Recipe", menuName = "ScritableObject/Recipe")]
public class ScriptableRecipe : ScriptableObject
{
    public Card card; // 制作出来的卡牌
    public List<RecipeMaterial> materials; // 制作需要的材料
    public RecipeType craftType; // 配方类型
    public int craftTime; // 制作时间
}