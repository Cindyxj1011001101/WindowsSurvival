using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CraftManager : IManager
{
    public static CraftManager Instance { get; } = new();

    private Dictionary<RecipeType, ScriptableRecipeLibrary> libraryDict = new(); // 以配方类型-配方库的形式存储所有可用配方

    private List<string> unlockedRecipes = new(); // 已解锁的合成配方

    public Dictionary<RecipeType, ScriptableRecipeLibrary> LibraryDict => libraryDict;

    private bool craftStopped;

    private CraftManager() { }

    public void Init()
    {
        if (libraryDict.IsNullOrEmpty())
        {
            // 加载每一种类型的配方库
            foreach (var library in Resources.LoadAll<ScriptableRecipeLibrary>("ScriptableObject/Craft/Libraries"))
            {
                libraryDict.Add(library.craftType, library);
            }
        }

        var techData = GameDataManager.Instance.TechnologyData;

        // 解锁一遍物品配方
        foreach (var techNode in Resources.LoadAll<ScriptableTechnologyNode>($"ScriptableObject/Technology"))
        {
            if (techData.studiedTechNodes.Contains(techNode.techName))
            {
                foreach (var recipe in techNode.recipes)
                {
                    UnlockRecipe(recipe.cardId);
                }
            }
        }
    }

    public void Reset()
    {
        craftStopped = false;
        unlockedRecipes.Clear();
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
    public void UnlockRecipe(string cardId)
    {
        if (unlockedRecipes.Contains(cardId)) return;

        unlockedRecipes.Add(cardId);
        EventManager.Instance.TriggerEvent(EventType.UnlockRecipe);
    }

    /// <summary>
    /// 判断能否合成指定配方
    /// </summary>
    /// <param name="recipe"></param>
    /// <param name="limitations"></param>
    /// <param name="hint"></param>
    /// <returns></returns>
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
        foreach (var material in GetMaterials(recipe))
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

    /// <summary>
    /// 得到建筑卡牌的建造限制
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
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
    /// 获取合成所需材料（考虑制作激励事件加成）
    /// </summary>
    /// <param name="recipe"></param>
    /// <returns></returns>
    public List<RecipeMaterial> GetMaterials(ScriptableRecipe recipe)
    {
        List<RecipeMaterial> result = new();

        // 制作激励事件进行中，材料需求减半
        var craftIncentive = GameEventManager.Instance.IsEventOngoing<CraftIncentive>();

        foreach (var m in recipe.materials)
        {
            var requiredNum = craftIncentive ? Mathf.CeilToInt((float)m.requiredNum / 2) : m.requiredNum;
            result.Add(new RecipeMaterial(m.cardId, requiredNum));
        }

        return result;
    }

    /// <summary>
    /// 得到合成所需时间（考虑制作激励事件加成）
    /// </summary>
    /// <param name="recipe"></param>
    /// <returns></returns>
    public int GetCraftTime(ScriptableRecipe recipe)
    {
        var craftIncentive = GameEventManager.Instance.IsEventOngoing<CraftIncentive>();

        return craftIncentive ? Mathf.CeilToInt((float)recipe.craftTime / 2) : recipe.craftTime;
    }

    /// <summary>
    /// 合成卡牌 (调用前请务必先判断能否合成)
    /// </summary>
    /// <param name="recipe"></param>
    public void Craft(ScriptableRecipe recipe, UnityAction<Card> dropCraftedCard, UnityAction<List<Card>> returnMaterials)
    {
        craftStopped = false;

        // 消耗合成材料
        PlayerBag playerBag = GameManager.Instance.PlayerBag;
        var materials = GetMaterials(recipe);
        foreach (var material in materials)
        {
            playerBag.DestroyCardsByCardId(material.cardId, material.requiredNum);
        }

        // 消耗时间
        TimeManager.Instance.AddTime(GetCraftTime(recipe), () =>
        {
            if (craftStopped)
            {
                // 此处返回表示制作失败了
                var toReturn = new List<Card>();
                // 返还材料
                foreach (var material in materials)
                {
                    toReturn.AddRange(CardFactory.CreateCards(material.cardId, material.requiredNum));
                }
                returnMaterials?.Invoke(toReturn);
                return;
            }

            // 制作成功
            // 创建一个新的卡牌
            var craftedCard = CardFactory.CreateCard(recipe.cardId);
            dropCraftedCard?.Invoke(craftedCard);

            // 触发制作事件
            EventManager.Instance.TriggerEvent(EventType.DialogueCondition, new SubscribeActionArgs("Craft", craftedCard.CardName));
        });
    }

    /// <summary>
    /// 外部调用停止合成（如麦麦被攻击时）
    /// </summary>
    public void StopCrafting()
    {
        if (craftStopped) return;

        craftStopped = true;
        // 停止时间流逝
        TimeManager.Instance.ShutTimePass();
    }
}