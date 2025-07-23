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
    public string cardId;
    public int requiredNum;
    public Sprite CardImage => CardFactory.GetCardImage(cardId);
}


[CreateAssetMenu(fileName = "Recipe", menuName = "ScriptableObject/Recipe")]
public class ScriptableRecipe : ScriptableObject
{
    public string cardId; // 制作出来的卡牌
    public RecipeType craftType; // 配方类型
    public List<RecipeMaterial> materials; // 制作需要的材料
    public int craftTime; // 制作时间

    private Card craftedCard;

    public Card CraftedCard
    {
        get
        {
            craftedCard ??= CardFactory.CreateCard(cardId);
            return craftedCard;
        }
    }

    public Sprite CardImage => CardFactory.GetCardImage(cardId);

    private void OnValidate()
    {
        cardId = name;
    }
}