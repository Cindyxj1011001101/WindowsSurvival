using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITechNode : HoverableButton
{
    public Text techName;
    public Transform recipeLayout;
    public StateSlider progressSlider;
    public GameObject background;
    public GameObject foreground_inProgress;
    public GameObject foreground_complished;
    public Text costText;
    public GameObject gifObject;

    private List<HoverableButton> recipeButtons = new();

    private ScriptableTechnologyNode techNode;

    public void RefreshDiplay()
    {
        if (this.techNode != null)
            DisplayTechNode(this.techNode);
    }

    public void DisplayTechNode(ScriptableTechnologyNode techNode)
    {
        this.techNode = techNode;

        // 显示必要信息
        techName.text = techNode.techName;
        costText.text = $"{techNode.cost}科技点";
        progressSlider.displayPercentage = false;
        progressSlider.SetValue(TechnologyManager.Instance.GetStudyProgress(techNode), techNode.cost);

        // 显示解锁配方
        MonoUtility.DestroyAllChildren(recipeLayout);
        recipeButtons.Clear();
        var recipeItem = Resources.Load<GameObject>("Prefabs/UI/Controls/Study/RecipeItem_TechNode");
        foreach (var recipe in techNode.recipes)
        {
            var obj = Instantiate(recipeItem, recipeLayout);
            var button = obj.GetComponent<HoverableButton>();
            button.normalImage.sprite = recipe.CardImage;
            recipeButtons.Add(button);
        }

        // 已完成
        if (TechnologyManager.Instance.IsTechNodeComplished(techNode))
        {
            background.SetActive(false);
            foreground_inProgress.SetActive(false);
            foreground_complished.SetActive(true);
            // 设置颜色
            foreach (var btn in recipeButtons)
            {
                btn.currentColor = btn.normalImage.color = ColorManager.Instance.cyan;
            }
        }
        // 未解锁
        else if (TechnologyManager.Instance.IsTechNodeLocked(techNode))
        {
            background.SetActive(true);
            foreground_inProgress.SetActive(false);
            foreground_complished.SetActive(false);
            // 设置颜色
            foreach (var btn in recipeButtons)
            {
                btn.currentColor = btn.normalImage.color = ColorManager.Instance.darkGrey;
            }
            techName.color = ColorManager.Instance.darkGrey;
        }
        // 正在研究
        else if (TechnologyManager.Instance.IsTechNodeBeingStudied(techNode))
        {
            background.SetActive(false);
            foreground_inProgress.SetActive(true);
            foreground_complished.SetActive(false);
            // 设置颜色
            foreach (var btn in recipeButtons)
            {
                btn.currentColor = btn.normalImage.color = ColorManager.Instance.white;
            }
            foreground_inProgress.GetComponent<Image>().color = ColorManager.Instance.white;
            gifObject.SetActive(true);
            gifObject.GetComponent<Animator>().SetTrigger("Play");
        }
        // 待研究
        else
        {
            background.SetActive(false);
            foreground_inProgress.SetActive(true);
            foreground_complished.SetActive(false);
            // 设置颜色
            foreach (var btn in recipeButtons)
            {
                btn.currentColor = btn.normalImage.color = ColorManager.Instance.white;
            }
            foreground_inProgress.GetComponent<Image>().color = ColorManager.Instance.lightGrey;
            gifObject.SetActive(false);
            techName.color = ColorManager.Instance.black;
        }
    }
}