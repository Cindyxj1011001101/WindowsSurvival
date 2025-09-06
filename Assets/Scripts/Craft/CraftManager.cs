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

    public bool CanCrfat(ScriptableRecipe recipe, out Dictionary<string, bool> limitations, out string hint)
    {
        limitations = null;

        // 建筑卡牌显示限制
        if (recipe.CardInstance.TryGetComponent<ConstructionComponent>(out var component))
        {
            limitations = GetConstructionLimitations(component);
            // 任意一项限制不满足不能建造
            foreach (var met in limitations.Values)
            {
                hint = "存在限制";
                if (!met) return false;
            }
        }

        // 配方未解锁，则无法合成
        if (IsRecipeLocked(recipe))
        {
            hint = "未解锁";
            return false;
        }

        // 数量达到上限，无法合成
        if (GlobalDataManager.Instance.GetCardNum(recipe.cardId) >= recipe.craftLimit)
        {
            hint = "已达上限";
            return false;
        }

        // 配方已解锁，看材料是否充足
        PlayerBag playerBag = GameManager.Instance.PlayerBag;
        foreach (var material in recipe.materials)
        {
            // 任何一项材料不满足数量需求，不能合成
            if (playerBag.GetTotalCountByCardId(material.cardId) < material.requiredNum)
            {
                hint = "材料不足";
                return false;
            }
        }

        hint = string.Empty;
        return true;
    }

    private Dictionary<string, bool> GetConstructionLimitations(ConstructionComponent component)
    {
        Dictionary<string, bool> result = new();

        var env = GameManager.Instance.CurEnvironmentBag;

        if (component.onlyInDoor)
        {
            result.Add("OnlyInDoor", env.PlaceData.isIndoor);
        }

        if (component.onlyOutDoor)
        {
            result.Add("OnlyOutDoor", !env.PlaceData.isIndoor);
        }

        if (component.onlyInWater)
        {
            result.Add("OnlyInWater", env.PlaceData.isInWater);
        }

        if (component.onlyOutWater)
        {
            result.Add("OnlyOutWater", !env.PlaceData.isInWater);
        }

        if (component.needCable)
        {
            result.Add("NeedCable", env.HasCable);
        }

        return result;
    }

    /// <summary>
    /// 合成卡牌 (调用前请务必先判断能否合成)
    /// </summary>
    /// <param name="recipe"></param>
    public void Craft(ScriptableRecipe recipe, Vector2 startPos)
    {
        // 合成一个物品
        PlayerBag playerBag = GameManager.Instance.PlayerBag;
        foreach (var material in recipe.materials)
        {
            playerBag.DestroyCardsByCardId(material.cardId, material.requiredNum);
        }

        // 消耗时间
        TimeManager.Instance.AddTime(recipe.craftTime);
        SoundManager.Instance.PlaySound("制作_03",true);

        // 创建一个新的卡牌
        var card = CardFactory.CreateCard(recipe.cardId);

        // 掉落制作出的卡牌
        // 如果是建筑卡牌或者是有内容物的卡牌，则优先掉落到环境里
        GameManager.Instance.AddCardWithTween(card, startPos, !(card.CardType == CardType.Construction || card.TryGetComponent<InnerContentsComponent>(out _)));

        EventManager.Instance.TriggerEvent(EventType.DialogueCondition, new SubscribeActionArgs("Craft", card.CardName));
    }
}