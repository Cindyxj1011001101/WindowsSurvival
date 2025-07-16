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

    private ScriptableTechnologyNode curSelectedTechNode; // ��¼��ǰѡ�еĿƼ��ڵ�

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
        // ��ʾ�Ƽ������ƺ�����
        techName.text = techNode.techName;
        techDescription.text = techNode.techDescription;

        // ��ʾ�Ƽ���ǰ���о���Ŀ
        MonoUtility.DestroyAllChildren(prerequisiteLayout);
        var techNodeEntry = Resources.Load<GameObject>("Prefabs/UI/Controls/TechNodeEntry");
        foreach (var prerequisite in techNode.prerequisites)
        {
            var content = Instantiate(techNodeEntry, prerequisiteLayout).GetComponentInChildren<Text>();
            content.text = $"���� {prerequisite.techName}";
        }

        // ��ʾ�Ƽ����Խ������䷽
        MonoUtility.DestroyAllChildren(recipeLayout);
        var recipeEntry = Resources.Load<GameObject>("Prefabs/UI/Controls/RecipeEntry");
        foreach (var recipe in techNode.recipes)
        {
            var obj = Instantiate(recipeEntry, recipeLayout);
            obj.transform.Find("Icon").GetComponent<Image>().sprite = recipe.card.cardImage;
            obj.GetComponentInChildren<Text>().text = recipe.card.cardName;
        }

        // ��ʾ�о���ť
        studyButton.gameObject.SetActive(!TechnologyManager.Instance.IsTechNodeStudied(techNode));

        // �о������
        if (TechnologyManager.Instance.IsTechNodeStudied(techNode))
        {
            studyButton.interactable = false;
            studyButton.GetComponentInChildren<Text>().text = "�����";
        }
        // �о����ڽ���
        else if (TechnologyManager.Instance.IsTechNodeBeingStudied(techNode))
        {
            studyButton.interactable = true;
            studyButton.GetComponentInChildren<Text>().text = "��ͣ�о�";
            // �����¼�����
            studyButton.onClick.RemoveAllListeners();
            studyButton.onClick.AddListener(() =>
            {
                // ��ͣ��ǰ�о�
                TechnologyManager.Instance.StopStudy();
                // ˢ����ʾ
                RefreshCurrentDisplay();
            });
        }
        // �о�δ����
        else if (TechnologyManager.Instance.IsTechNodeLocked(techNode))
        {
            studyButton.interactable = false;
            studyButton.GetComponentInChildren<Text>().text = "δ����";
        }
        // �������о��ڽ���
        else if (TechnologyManager.Instance.IsAnyTechNodeBeingStudied())
        {
            studyButton.interactable = false;
            studyButton.GetComponentInChildren<Text>().text = "�о�";
        }
        // �����о�
        else
        {
            studyButton.interactable = true;
            studyButton.GetComponentInChildren<Text>().text = "�о�";
            // �����¼�����
            studyButton.onClick.RemoveAllListeners();
            studyButton.onClick.AddListener(() =>
            {
                // ��ͣ��ǰ�о�
                TechnologyManager.Instance.StopStudy();
                // �о���ǰ�Ƽ��ڵ�
                // �����ť��ʼ�о�
                TechnologyManager.Instance.Study(techNode);
                // ˢ����ʾ
                RefreshCurrentDisplay();
            });
        }

        // ��ʾ�о����Ⱥ��о��ٶ�
        // �о������
        if (TechnologyManager.Instance.IsTechNodeStudied(techNode))
        {
            progressSlider.value = 1;
            progressSlider.GetComponentInChildren<Text>().text = $"�����";
        }
        // �������
        else
        {
            var progress = TechnologyManager.Instance.GetStudyProgress(techNode);
            progressSlider.value = progress / techNode.cost;
            progressSlider.GetComponentInChildren<Text>().text = $"{progress} / {techNode.cost}";
        }

        // ��ʾ�о��ٶ�
        if (TechnologyManager.Instance.IsTechNodeBeingStudied(techNode))
        {
            studyRate.gameObject.SetActive(true);
            studyRate.text = $"+ {TechnologyManager.Instance.CurStudyRate:0.0} �Ƽ��� / 15 ����";
        }
        // �о����ڽ���
        else if (TechnologyManager.Instance.IsTechNodeBeingStudied(techNode))
        {
            progressSlider.value = TechnologyManager.Instance.CurProgress / techNode.cost;
            progressSlider.GetComponentInChildren<Text>().text = $"{TechnologyManager.Instance.CurProgress} / {techNode.cost}";
            studyRate.gameObject.SetActive(true);
            studyRate.text = $"{TechnologyManager.Instance.CurStudyRate} / 15 ����";
        }
        // ���������о������
        else
        {
            studyRate.gameObject.SetActive(false);
        }
    }
}
