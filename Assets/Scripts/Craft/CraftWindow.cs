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
    }

    protected override void Init()
    {
        DisplayRecipeTypes();
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<ChangePlayerBagCardsArgs>(EventType.ChangePlayerBagCards, RefreshCurrentDisplay);
    }

    private void RefreshCurrentDisplay(ChangePlayerBagCardsArgs args)
    {
        if (recipieLayout.childCount > 0)
        {
            DisplayRecipesByType(currentRecipeType, true); // 传递true表示是刷新操作
        }
    }

    private void DisplayRecipeTypes()
    {
        var recipeTypeButtonPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/RecipeTypeButton");

        DestroyAllChildren(leftBar);

        var group = leftBar.GetComponent<ToggleGroup>();

        foreach (var (type, library) in CraftManager.Instance.LibraryDict)
        {
            var buttonObj = Instantiate(recipeTypeButtonPrefab, leftBar);
            var button = buttonObj.GetComponent<CustomButton>();
            button.group = group;
            var text = buttonObj.GetComponentInChildren<Text>();
            text.text = type.ToString();
            button.onSelect.AddListener(() =>
            {
                currentRecipeType = type;
                currentSelectedRecipe = null; // 切换类型时清空选中记录
                DisplayRecipesByType(type);
            });
        }
    }

    private void DisplayRecipesByType(RecipeType recipeType, bool isRefresh = false)
    {
        var recipeButtonPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/RecipeButton");

        DestroyAllChildren(recipieLayout);

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
            recipeButton.DisplayRecipe(recipe.cardData.cardImage,
                                    CraftManager.Instance.IsRecipeLocked(recipe),
                                    CraftManager.Instance.CanCrfat(recipe));
            button.onSelect.AddListener(() =>
            {
                currentSelectedRecipe = recipe; // 记录选中的配方
                DisplayRecipeDetails(recipe);
            });

            // 如果是刷新，继续选中上一个选中的配方
            if (isRefresh && recipe == currentSelectedRecipe)
                button.isOn = true;
        }

        //// 尝试重新选择之前选中的配方
        //bool foundPreviousSelection = false;
        //if (isRefresh && currentSelectedRecipe != null)
        //{
        //    for (int i = 0; i < recipieLayout.childCount; i++)
        //    {
        //        var button = recipieLayout.GetChild(i).GetComponent<CustomButton>();
        //        var recipeButton = button.GetComponent<UIRecipeButton>();

        //        // 这里需要根据你的实际实现来判断是否是同一个配方
        //        // 假设UIRecipeButton有一个GetRecipe方法或者我们可以通过其他方式比较
        //        if (IsSameRecipe(recipeButton, currentSelectedRecipe))
        //        {
        //            button.isOn = true;
        //            button.onSelect.Invoke();
        //            foundPreviousSelection = true;
        //            break;
        //        }
        //    }
        //}

        //// 如果没有找到之前选中的配方，选择第一个
        //if (!foundPreviousSelection && recipieLayout.childCount > 0)
        //{
        //    var firstButton = recipieLayout.GetChild(0).GetComponent<CustomButton>();
        //    firstButton.isOn = true;
        //    firstButton.onSelect.Invoke();
        //}
    }

    private void DisplayRecipeDetails(ScriptableRecipe recipe)
    {
        var recipeMaterialPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/RecipeMaterial");

        DestroyAllChildren(materialLayout);
        // 显示卡牌图标
        cardIcon.sprite = recipe.cardData.cardImage;

        // 显示卡牌名称
        cardNameText.text = recipe.cardData.cardName;

        // 显示卡牌描述
        cradDescriptionText.text = recipe.cardData.cardDesc;

        // 显示所需材料
        foreach (var material in recipe.materials)
        {
            var recipeMaterial = Instantiate(recipeMaterialPrefab, materialLayout).GetComponent<UIRecipeMaterial>();
            recipeMaterial.DisplayMaterial(material.cardData.cardImage, material.requiredAmount, GameManager.Instance.PlayerBag.GetTotalCountOfSpecificCard(material.cardData));
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

        // 添加制作事件
        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(() =>
        {
            // 合成卡牌
            CraftManager.Instance.Craft(recipe);
            // 刷新显示
            DisplayRecipeDetails(recipe);
            // 刷新配方列表
            DisplayRecipesByType(currentRecipeType, true);
        });
    }

    private void DestroyAllChildren(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }
}