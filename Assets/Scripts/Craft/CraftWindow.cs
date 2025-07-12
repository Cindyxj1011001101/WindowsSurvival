using System.Text;
using UnityEngine;
using UnityEngine.UI;

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

    //protected override void Awake()
    //{
    //    base.Awake();
    //    DestroyAllChildren(leftBar);
    //    DestroyAllChildren(recipieLayout);
    //    DestroyAllChildren(materialLayout);
    //}

    protected override void Init()
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
            button.onSelect.AddListener(() => DisplayRecipesByType(type));
        }
    }

    private void DisplayRecipesByType(RecipeType recipeType)
    {
        var recipeButtonPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/RecipeButton");

        DestroyAllChildren(recipieLayout);

        var group = recipieLayout.GetComponent<ToggleGroup>();

        foreach (var recipe in CraftManager.Instance.LibraryDict[recipeType].recipes)
        {
            var recipeButtonObj = Instantiate(recipeButtonPrefab, recipieLayout);
            var button = recipeButtonObj.GetComponent<CustomButton>();
            button.group = group;
            var recipeButton = recipeButtonObj.GetComponent<UIRecipeButton>();
            recipeButton.DisplayRecipe(recipe.cardData.cardImage, CraftManager.Instance.IsRecipeLocked(recipe), CraftManager.Instance.CanCrfat(recipe));
            button.onSelect.AddListener(() => DisplayRecipeDetails(recipe));
        }
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