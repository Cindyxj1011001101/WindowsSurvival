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
    Equipment = 5,
    Construction = 6,
}

[Serializable]
public class RecipeMaterial
{
    public string cardName;
    public int requiredAmount;
    public Sprite CardImage => Resources.Load<Sprite>("Sprites/" + cardName);
}


[CreateAssetMenu(fileName = "Recipe", menuName = "ScriptableObject/Recipe")]
public class ScriptableRecipe : ScriptableObject
{
    public string cardName; // 制作出来的卡牌
    public string cardDesc; // 卡牌描述
    public RecipeType craftType; // 配方类型
    public List<RecipeMaterial> materials; // 制作需要的材料
    public int craftTime; // 制作时间

    public Sprite CardImage => Resources.Load<Sprite>("Sprites/" + cardName);

    private void OnValidate()
    {
        cardName = name;
    }
}