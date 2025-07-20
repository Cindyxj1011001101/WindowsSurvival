using System.Collections.Generic;
using UnityEngine;

public class CraftManager
{
    private static CraftManager instance = new();
    public static CraftManager Instance => instance;

    private Dictionary<RecipeType, ScriptableRecipeLibrary> libraryDict = new(); // 以配方类型-配方库的形式存储所有可用配方

    private List<string> unlockedRecipes = new(); // 已解锁的合成配方

    public Dictionary<RecipeType, ScriptableRecipeLibrary> LibraryDict => libraryDict;
    public List<string> UnlockedRecipes => unlockedRecipes;

    private CraftManager()
    {
        // 加载每一种类型的配方库
        foreach (var library in Resources.LoadAll<ScriptableRecipeLibrary>("ScriptableObject/Craft/Libraries"))
        {
            libraryDict.Add(library.craftType, library);
        }
        // 加载已解锁的配方
        unlockedRecipes = GameDataManager.Instance.UnlockedRecipes;
    }

    /// <summary>
    /// 判断合成配方是否解锁
    /// </summary>
    /// <param name="recipe"></param>
    /// <returns></returns>
    public bool IsRecipeLocked(ScriptableRecipe recipe)
    {
        return !unlockedRecipes.Contains(recipe.cardId);
    }

    /// <summary>
    /// 解锁指定的合成配方
    /// </summary>
    /// <param name="recipe"></param>
    public void UnlockRecipe(ScriptableRecipe recipe)
    {
        if (unlockedRecipes.Contains(recipe.cardId)) return;

        unlockedRecipes.Add(recipe.cardId);
        EventManager.Instance.TriggerEvent(EventType.UnlockRecipe);
    }

    /// <summary>
    /// 判断一个配方能否合成
    /// </summary>
    /// <param name="recipe"></param>
    public bool CanCrfat(ScriptableRecipe recipe)
    {
        // 配方未解锁，则无法合成
        if (IsRecipeLocked(recipe)) return false;

        // 配方已解锁
        // 能否合成取决于材料是否充足
        PlayerBag playerBag = GameManager.Instance.PlayerBag;
        foreach (var material in recipe.materials)
        {
            // 任何一项材料不满足数量需求，不能合成
            if (playerBag.GetTotalCountByCardId(material.cardId) < material.requiredAmount) return false;
        }

        return true;
    }

    /// <summary>
    /// 合成卡牌 (调用前请务必先判断能否合成)
    /// </summary>
    /// <param name="recipe"></param>
    public void Craft(ScriptableRecipe recipe)
    {
        // 合成一个物品
        PlayerBag playerBag = GameManager.Instance.PlayerBag;
        foreach (var material in recipe.materials)
        {
            playerBag.RemoveCardsByCardId(material.cardId, material.requiredAmount);
        }

        // 创建一个新的卡牌
        var card = CardFactory.CreateCard(recipe.cardId);

        // 掉落制作出的卡牌
        // 如果是建筑卡牌，则优先掉落到环境里
        GameManager.Instance.AddCard(card, card.cardType != CardType.Construction);

        // 消耗时间
        TimeManager.Instance.AddTime(recipe.craftTime);
    }
}