using System;
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
        //foreach (var node in techNodes)
        //{
        //    GameDataManager.Instance.TechnologyData.techNodeDict.Add(node.gameObject.name, new TechNodeData { name = node.gameObject.name, progress = 0 });
        //}
        //GameDataManager.Instance.SaveTechnologyData();
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
        studyButton.gameObject.SetActive(!TechnologyManager.Instance.IsTechNodeStudied(techNode));

        // 研究已完成
        if (TechnologyManager.Instance.IsTechNodeStudied(techNode))
        {
            studyButton.interactable = false;
            studyButton.GetComponentInChildren<Text>().text = "已完成";
        }
        // 研究正在进行
        else if (TechnologyManager.Instance.IsTechNodeBeingStudied(techNode))
        {
            studyButton.interactable = true;
            studyButton.GetComponentInChildren<Text>().text = "暂停研究";
            // 添加事件监听
            studyButton.onClick.RemoveAllListeners();
            studyButton.onClick.AddListener(() =>
            {
                // 暂停当前研究
                TechnologyManager.Instance.StopStudy();
                // 刷新显示
                RefreshCurrentDisplay();
            });
        }
        // 研究未解锁
        else if (TechnologyManager.Instance.IsTechNodeLocked(techNode))
        {
            studyButton.interactable = false;
            studyButton.GetComponentInChildren<Text>().text = "未解锁";
        }
        else
        {
            studyButton.interactable = true;
            studyButton.GetComponentInChildren<Text>().text = "研究";
            // 添加事件监听
            studyButton.onClick.RemoveAllListeners();
            studyButton.onClick.AddListener(() =>
            {
                // 暂停当前研究
                TechnologyManager.Instance.StopStudy();
                // 研究当前科技节点
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
            progressSlider.GetComponentInChildren<Text>().text = $"已完成";
        }
        // 其他情况
        else
        {
            var progress = TechnologyManager.Instance.GetStudyProgress(techNode);
            progressSlider.value = progress / techNode.cost;
            progressSlider.GetComponentInChildren<Text>().text = $"{progress} / {techNode.cost}";
        }

        // 显示研究速度
        if (TechnologyManager.Instance.IsTechNodeBeingStudied(techNode))
        {
            studyRate.gameObject.SetActive(true);
            studyRate.text = $"+ {TechnologyManager.Instance.CurStudyRate:0.0} 科技点 / 15 分钟";
        }
        else
        {
            studyRate.gameObject.SetActive(false);
        }
    }
}
