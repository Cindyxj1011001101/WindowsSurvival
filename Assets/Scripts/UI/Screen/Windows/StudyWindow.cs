using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StudyWindow : WindowBase
{
    [SerializeField] Text techName;
    [SerializeField] Text techDescription;
    [SerializeField] Transform prerequisiteLayout;
    [SerializeField] Transform recipeLayout;

    [SerializeField] Button studyButton;
    [SerializeField] Slider progressSlider;
    [SerializeField] Text studyRate;

    [SerializeField] ToggleGroup techNodesGroup;
    [SerializeField] List<UITechNode> techNodes;

    private ScriptableTechnologyNode curSelectedTechNode; // 记录当前选中的科技节点

    protected override void Awake()
    {
        base.Awake();
        EventManager.Instance.AddListener(EventType.ChangeStudyProgress, RefreshCurrentDisplay);
    }
    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EventType.ChangeStudyProgress, RefreshCurrentDisplay);
    }

    protected override void Init()
    {
        DisplayTechTree();
    }

    private void DisplayTechTree()
    {
        foreach (var node in techNodes)
        {
            var techNodeSO = Resources.Load<ScriptableTechnologyNode>("ScriptableObject/Technology/" + node.gameObject.name);
            node.DisplayTechNode(techNodeSO);
            var button = node.GetComponent<CustomButton>();
            button.group = techNodesGroup;
            button.onSelect.AddListener(() =>
            {
                DisplayTechNodeDetails(techNodeSO);
                curSelectedTechNode = techNodeSO;
            });
        }
    }
    private void RefreshCurrentDisplay()
    {
        foreach (var node in techNodes)
        {
            var techNodeSO = Resources.Load<ScriptableTechnologyNode>("ScriptableObject/Technology/" + node.gameObject.name);
            node.DisplayTechNode(techNodeSO);
        }
        DisplayTechNodeDetails(curSelectedTechNode);
    }

    private void DisplayTechNodeDetails(ScriptableTechnologyNode techNode)
    {
        // 显示科技的名称和描述
        techName.text = techNode.techName;
        techDescription.text = techNode.techDescription;

        // 显示科技的前置研究项目
        MonoUtility.DestroyAllChildren(prerequisiteLayout);
        var techNodeEntry = Resources.Load<GameObject>("Prefabs/UI/Controls/TechNodeEntry");
        foreach (var prerequisite in techNode.prerequisites)
        {
            var content = Instantiate(techNodeEntry, prerequisiteLayout).GetComponentInChildren<Text>();
            content.text = $"—— {prerequisite.techName}";
        }

        // 显示科技可以解锁的配方
        MonoUtility.DestroyAllChildren(recipeLayout);
        var recipeEntry = Resources.Load<GameObject>("Prefabs/UI/Controls/RecipeEntry");
        foreach (var recipe in techNode.recipes)
        {
            var obj = Instantiate(recipeEntry, recipeLayout);
            obj.transform.Find("Icon").GetComponent<Image>().sprite = recipe.cardData.cardImage;
            obj.GetComponentInChildren<Text>().text = recipe.cardData.cardName;
        }

        // 显示研究按钮
        // 研究已完成
        if (TechnologyManager.Instance.IsTechNodeStudied(techNode))
        {
            studyButton.interactable = false;
            studyButton.GetComponentInChildren<Text>().text = "已完成";
        }
        // 研究正在进行
        else if (TechnologyManager.Instance.IsTechNodeBeingStudied(techNode))
        {
            studyButton.interactable = false;
            studyButton.GetComponentInChildren<Text>().text = "研究中";
        }
        // 研究未解锁
        else if (TechnologyManager.Instance.IsTechNodeLocked(techNode))
        {
            studyButton.interactable = false;
            studyButton.GetComponentInChildren<Text>().text = "未解锁";
        }
        // 有其他研究在进行
        else if (TechnologyManager.Instance.IsAnyTechNodeBeingStudied())
        {
            studyButton.interactable = false;
            studyButton.GetComponentInChildren<Text>().text = "研究";
        }
        // 可以研究
        else
        {
            studyButton.interactable = true;
            studyButton.GetComponentInChildren<Text>().text = "研究";
            // 添加事件监听
            studyButton.onClick.RemoveAllListeners();
            studyButton.onClick.AddListener(() =>
            {
                // 点击按钮开始研究
                TechnologyManager.Instance.Study(techNode);
                // 刷新显示
                RefreshCurrentDisplay();
            });
        }

        // 显示研究进度和研究速度
        // 研究已完成
        if (TechnologyManager.Instance.IsTechNodeStudied(techNode))
        {
            progressSlider.value = 1;
            progressSlider.GetComponentInChildren<Text>().text = $"{techNode.cost} / {techNode.cost}";
            studyRate.gameObject.SetActive(false);
        }
        // 研究正在进行
        else if (TechnologyManager.Instance.IsTechNodeBeingStudied(techNode))
        {
            progressSlider.value = TechnologyManager.Instance.CurProgress / techNode.cost;
            progressSlider.GetComponentInChildren<Text>().text = $"{TechnologyManager.Instance.CurProgress} / {techNode.cost}";
            studyRate.gameObject.SetActive(true);
            studyRate.text = $"{TechnologyManager.Instance.CurStudyRate} / 15 分钟";
        }
        // 其他不能研究的情况
        else
        {
            progressSlider.value = 0;
            progressSlider.GetComponentInChildren<Text>().text = $"0 / {techNode.cost}";
            studyRate.gameObject.SetActive(false);
        }
    }
}
