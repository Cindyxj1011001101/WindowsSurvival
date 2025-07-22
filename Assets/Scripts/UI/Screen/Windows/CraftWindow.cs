using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class CraftWindow : WindowBase
{
    [SerializeField] private Transform recipeLibraryLayout;
    [SerializeField] private Transform recipieLayout;
    [SerializeField] private Transform materialLayout;
    [SerializeField] private CardSlot slot;
    [SerializeField] private Text craftTimeText;
    [SerializeField] private CraftButton craftButton;
    [SerializeField] private RectTransform selectRect;

    private RecipeType currentRecipeType;
    private ScriptableRecipe currentSelectedRecipe; // 记录当前选中的配方

    private Sequence currentAnimation;

    private Dictionary<RecipeType, RectTransform> recipeLibraryItemTransforms = new();

    protected override void Awake()
    {
        base.Awake();

        // 注册背包变化事件
        EventManager.Instance.AddListener<ChangePlayerBagCardsArgs>(EventType.ChangePlayerBagCards, RefreshCurrentDisplay);
        EventManager.Instance.AddListener(EventType.UnlockRecipe, RefreshCurrentDisplay);
    }

    protected override void Init()
    {
        var firstChild = recipeLibraryLayout.GetChild(0);
        //selectRect.anchoredPosition = new(selectRect.anchoredPosition.x, (firstChild.transform as RectTransform).anchoredPosition.y);
        currentRecipeType = (RecipeType)Enum.Parse(typeof(RecipeType), firstChild.name);
        DisplayRecipeLibraries();
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<ChangePlayerBagCardsArgs>(EventType.ChangePlayerBagCards, RefreshCurrentDisplay);
        EventManager.Instance.RemoveListener(EventType.UnlockRecipe, RefreshCurrentDisplay);
    }

    private void RefreshCurrentDisplay(ChangePlayerBagCardsArgs args)
    {
        RefreshCurrentDisplay();
    }

    private void RefreshCurrentDisplay()
    {
        DisplayRecipesByType(currentRecipeType, true); // 传递true表示是刷新操作
    }

    /// <summary>
    /// 显示配方类别
    /// </summary>
    private void DisplayRecipeLibraries()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(recipeLibraryLayout.transform as RectTransform);

        for (int i = 0; i < recipeLibraryLayout.childCount; i++)
        {
            var button = recipeLibraryLayout.GetChild(i).GetComponent<HoverableButton>();
            button.onClick.RemoveAllListeners();
            RecipeType type = (RecipeType)Enum.Parse(typeof(RecipeType), button.name);
            button.onClick.AddListener(() =>
            {
                currentRecipeType = type;
                currentSelectedRecipe = null; // 切换类型时清空选中记录
                DisplayRecipesByType(type);
            });
            recipeLibraryItemTransforms.Add(type, button.transform as RectTransform);
        }

        DisplayRecipesByType(currentRecipeType);
    }

    private void SelectRecipeLibraryWithTween(RecipeType type)
    {
        // 停止当前动画
        if (currentAnimation != null && currentAnimation.IsActive())
        {
            currentAnimation.Kill();
        }

        Vector2 targetPos = new(selectRect.anchoredPosition.x, recipeLibraryItemTransforms[type].anchoredPosition.y);

        // 创建动画序列
        currentAnimation = DOTween.Sequence();

        currentAnimation.Append(selectRect.DOAnchorPos(targetPos, 0.2f).SetEase(Ease.OutQuad));
    }

    /// <summary>
    /// 显示某一类的所有配方
    /// </summary>
    /// <param name="recipeType"></param>
    /// <param name="isRefresh"></param>
    private void DisplayRecipesByType(RecipeType recipeType, bool isRefresh = false)
    {
        var recipeButtonPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/Craft/RecipeItem");

        MonoUtility.DestroyAllChildren(recipieLayout);

        // 获取当前类型的配方列表
        var recipes = CraftManager.Instance.LibraryDict[recipeType].recipes;

        // 对配方进行排序：可合成 > 不可合成 > 未解锁
        var sortedRecipes = recipes.OrderBy(recipe =>
        {
            if (CraftManager.Instance.IsRecipeLocked(recipe))
            {
                return 2; // 未解锁的排在最后
            }
            else if (!CraftManager.Instance.CanCrfat(recipe))
            {
                return 1; // 不可合成的排在中间
            }
            else
            {
                return 0; // 可合成的排在最前
            }
        }).ToList();

        // 创建所有配方按钮
        foreach (var recipe in sortedRecipes)
        {
            var recipeItemObj = Instantiate(recipeButtonPrefab, recipieLayout);
            var button = recipeItemObj.GetComponent<HoverableButton>();
            var recipeItem = recipeItemObj.GetComponent<UIRecipeItem>();
            recipeItem.DisplayRecipe(
                recipe.CardImage,
                CraftManager.Instance.IsRecipeLocked(recipe),
                CraftManager.Instance.CanCrfat(recipe)
                );
            button.onClick.AddListener(() =>
            {
                currentSelectedRecipe = recipe; // 记录选中的配方
                DisplayRecipeDetails(recipe);
            });

            // 如果是刷新，继续选中上一个选中的配方
            if (isRefresh && recipe == currentSelectedRecipe)
                DisplayRecipeDetails(recipe);
        }

        if (currentSelectedRecipe == null)
            DisplayRecipeDetails(sortedRecipes[0]);

        MonoUtility.UpdateContainerHeight(recipieLayout.GetComponent<GridLayoutGroup>(), recipes.Count);

        // 播放选择动效
        SelectRecipeLibraryWithTween(recipeType);
    }

    /// <summary>
    /// 显示具体的配方信息
    /// </summary>
    /// <param name="recipe"></param>
    private void DisplayRecipeDetails(ScriptableRecipe recipe)
    {
        var recipeMaterialPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/Craft/MaterialItem");

        MonoUtility.DestroyAllChildren(materialLayout);

        // 显示卡牌
        slot.ClearSlot();
        slot.DisplayCard(recipe.CraftedCard, 1, false);

        // 显示所需材料
        foreach (var material in recipe.materials)
        {
            var recipeMaterial = Instantiate(recipeMaterialPrefab, materialLayout).GetComponent<UIRecipeMaterial>();
            recipeMaterial.DisplayMaterial(
                material.CardImage,
                material.requiredNum,
                GameManager.Instance.PlayerBag.GetTotalCountByCardId(material.cardId)
                );
        }

        // 显示制作时间
        int hour = recipe.craftTime / 60;
        int minute = recipe.craftTime % 60;
        StringBuilder sb = new();
        sb.Append(hour > 0 ? $"{hour}h" : "");
        sb.Append(minute > 0 ? $"{minute}min" : "");
        craftTimeText.text = sb.ToString();

        // 显示制作按钮
        craftButton.DisplayButton(CraftManager.Instance.IsRecipeLocked(recipe), CraftManager.Instance.CanCrfat(recipe));

        // 添加制作事件
        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(() =>
        {
            // 合成卡牌
            CraftManager.Instance.Craft(recipe);
            // 刷新显示
            RefreshCurrentDisplay();
        });
    }
}