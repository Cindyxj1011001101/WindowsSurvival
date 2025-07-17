using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CraftWindow : WindowBase
{
    [SerializeField] private Transform leftBar;
    [SerializeField] private Transform recipieLayout;
    [SerializeField] private Transform materialLayout;
    [SerializeField] private Image cardIcon;
    [SerializeField] private Text craftTimeText;
    [SerializeField] private Text cardNameText;
    [SerializeField] private Text cradDescriptionText;
    [SerializeField] private Button craftButton;

    private RecipeType currentRecipeType;
    private ScriptableRecipe currentSelectedRecipe; // 记录当前选中的配方

    protected override void Awake()
    {
        base.Awake();

        // 注册背包变化事件
        EventManager.Instance.AddListener<ChangePlayerBagCardsArgs>(EventType.ChangePlayerBagCards, RefreshCurrentDisplay);
        EventManager.Instance.AddListener(EventType.UnlockRecipe, RefreshCurrentDisplay);
    }

    protected override void Init()
    {
        DisplayRecipeTypes();
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
    private void DisplayRecipeTypes()
    {
        var recipeTypeButtonPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/RecipeTypeButton");

        MonoUtility.DestroyAllChildren(leftBar);

        var group = leftBar.GetComponent<ToggleGroup>();

        foreach (var (type, library) in CraftManager.Instance.LibraryDict)
        {
            var buttonObj = Instantiate(recipeTypeButtonPrefab, leftBar);
            var button = buttonObj.GetComponent<CustomButton>();
            button.group = group;
            var text = buttonObj.GetComponentInChildren<Text>();
            text.text = library.libraryName;
            button.onSelect.AddListener(() =>
            {
                currentRecipeType = type;
                currentSelectedRecipe = null; // 切换类型时清空选中记录
                DisplayRecipesByType(type);
            });
        }
    }

    /// <summary>
    /// 显示某一类的所有配方
    /// </summary>
    /// <param name="recipeType"></param>
    /// <param name="isRefresh"></param>
    private void DisplayRecipesByType(RecipeType recipeType, bool isRefresh = false)
    {
        var recipeButtonPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/RecipeButton");

        MonoUtility.DestroyAllChildren(recipieLayout);

        var group = recipieLayout.GetComponent<ToggleGroup>();

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
            var recipeButtonObj = Instantiate(recipeButtonPrefab, recipieLayout);
            var button = recipeButtonObj.GetComponent<CustomButton>();
            button.group = group;
            var recipeButton = recipeButtonObj.GetComponent<UIRecipeButton>();
            recipeButton.DisplayRecipe(
                recipe.CardImage,
                CraftManager.Instance.IsRecipeLocked(recipe),
                CraftManager.Instance.CanCrfat(recipe)
                );
            button.onSelect.AddListener(() =>
            {
                currentSelectedRecipe = recipe; // 记录选中的配方
                DisplayRecipeDetails(recipe);
            });

            // 如果是刷新，继续选中上一个选中的配方
            if (isRefresh && recipe == currentSelectedRecipe)
                button.isOn = true;
        }

        MonoUtility.UpdateContainerHeight(recipieLayout.GetComponent<GridLayoutGroup>(), recipes.Count);
    }

    /// <summary>
    /// 显示具体的配方信息
    /// </summary>
    /// <param name="recipe"></param>
    private void DisplayRecipeDetails(ScriptableRecipe recipe)
    {
        var recipeMaterialPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/RecipeMaterial");

        MonoUtility.DestroyAllChildren(materialLayout);
        // 显示卡牌图标
        cardIcon.sprite = recipe.CardImage;

        // 显示卡牌名称
        cardNameText.text = recipe.cardName;

        // 显示卡牌描述
        cradDescriptionText.text = recipe.cardDesc;

        // 显示所需材料
        foreach (var material in recipe.materials)
        {
            var recipeMaterial = Instantiate(recipeMaterialPrefab, materialLayout).GetComponent<UIRecipeMaterial>();
            recipeMaterial.DisplayMaterial(
                material.CardImage,
                material.requiredAmount,
                GameManager.Instance.PlayerBag.GetTotalCountOfSpecificCard(material.cardName)
                );
        }

        // 显示制作时间
        int hour = recipe.craftTime / 60;
        int minute = recipe.craftTime % 60;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(hour > 0 ? $"{hour} 时 " : "");
        sb.AppendLine($"{minute} 分");
        craftTimeText.text = sb.ToString();

        // 显示制作按钮
        craftButton.interactable = CraftManager.Instance.CanCrfat(recipe);
        if (CraftManager.Instance.IsRecipeLocked(recipe))
            craftButton.GetComponentInChildren<Text>().text = "未解锁";
        else
            craftButton.GetComponentInChildren<Text>().text = "制作";

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