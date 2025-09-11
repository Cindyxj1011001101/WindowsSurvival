using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITechNode : HoverableButton
{
    public Text techName;
    public Transform recipeLayout;
    public UIStateSlider progressSlider;
    public GameObject background;
    public GameObject foreground_inProgress;
    public GameObject foreground_complished;
    public Text costText;
    public GameObject gifObject;

    public GameObject recipeItemPrefab;

    private List<HoverableButton> recipeButtons = new();

    public void DisplayTechNode(ScriptableTechnologyNode techNode)
    {
        bool complished = TechnologyManager.Instance.IsTechNodeComplished(techNode);
        bool beingStudied = TechnologyManager.Instance.IsTechNodeBeingStudied(techNode);
        bool locked = TechnologyManager.Instance.IsTechNodeLocked(techNode);

        // 显示必要信息
        techName.text = techNode.techName;
        costText.text = $"{techNode.cost}科技点";
        progressSlider.displayPercentage = false;
        progressSlider.SetValue(TechnologyManager.Instance.GetStudyProgress(techNode), techNode.cost);

        // 显示解锁配方
        ObjectBufferPool.Instance.RestoreAllChildren(recipeLayout);
        recipeButtons.Clear();

        HoverableButton button;
        HoverTipController tipController;
        foreach (var recipe in techNode.recipes)
        {
            button = ObjectBufferPool.Instance.Get(recipeItemPrefab, recipeLayout).GetComponent<HoverableButton>();
            button.normalImage.sprite = recipe.CardImage;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                (WindowsManager.Instance.OpenWindow("Details") as DetailsWindow).Display(recipe.CardInstance, DisplayType.DetailsAndCraftButton);
            });

            tipController = button.GetComponent<HoverTipController>();
            tipController.SetTip(recipe.CardInstance.CardName);
            recipeButtons.Add(button);
        }

        // 已完成
        if (complished)
        {
            background.SetActive(false);
            foreground_inProgress.SetActive(false);
            foreground_complished.SetActive(true);
            // 设置颜色
            foreach (var btn in recipeButtons)
            {
                btn.hoveredColor = btn.currentColor = btn.normalImage.color = ColorManager.Cyan;
            }
        }
        // 未解锁
        else if (locked)
        {
            background.SetActive(true);
            foreground_inProgress.SetActive(false);
            foreground_complished.SetActive(false);
            // 设置颜色
            foreach (var btn in recipeButtons)
            {
                btn.currentColor = btn.normalImage.color = ColorManager.DarkGrey;
            }
            techName.color = ColorManager.DarkGrey;
        }
        // 正在研究
        else if (beingStudied)
        {
            background.SetActive(false);
            foreground_inProgress.SetActive(true);
            foreground_complished.SetActive(false);
            // 设置颜色
            foreach (var btn in recipeButtons)
            {
                btn.currentColor = btn.normalImage.color = ColorManager.White;
            }
            foreground_inProgress.GetComponent<Image>().color = ColorManager.White;
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
                btn.currentColor = btn.normalImage.color = ColorManager.LightGrey;
            }
            foreground_inProgress.GetComponent<Image>().color = ColorManager.LightGrey;
            gifObject.SetActive(false);
            techName.color = ColorManager.Black;
        }
    }
}