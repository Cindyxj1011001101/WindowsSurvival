using DG.Tweening;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class StudyWindow : WindowBase
{
    [SerializeField] private Text techName;
    [SerializeField] private Text techDescription;

    [SerializeField] private StudyButton studyButton;
    [SerializeField] private StateSlider progressSlider;
    [SerializeField] private Text studyRate;
    [SerializeField] private Text studyTime;

    [SerializeField] private Transform detailLayout;
    [SerializeField] private Transform menuLayout;
    [SerializeField] private Transform content;

    [SerializeField] private GameObject prerequisite;
    [SerializeField] private GameObject unlockRecipe;

    [SerializeField] private RectTransform selectRect;

    private string curSelectedType; // 当前选择的科技类型
    private ScriptableTechnologyNode curSelectedTechNode; // 记录当前选中的科技节点

    private List<GameObject> temp = new();

    private Sequence curAnim;

    private Dictionary<string, RectTransform> menuItemTransforms = new();

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
        //DisplayTechTree();
        LayoutRebuilder.ForceRebuildLayoutImmediate(menuLayout as RectTransform);

        curSelectedType = menuLayout.GetChild(0).name;

        menuItemTransforms.Clear();
        for (int i = 0; i < menuLayout.childCount; i++)
        {
            var child = menuLayout.GetChild(i);
            var button = child.GetComponent<HoverableButton>();
            button.onClick.AddListener(() =>
            {
                curSelectedType = child.name;
                curSelectedTechNode = null;
                DisplayTechTree(child.name);
            });
            menuItemTransforms.Add(child.name, child as RectTransform);
        }

        DisplayTechTree(curSelectedType);
    }

    private void DisplayTechTree(string type, bool isRefresh = false)
    {
        // 只显示对应类型的科技节点
        Transform targetChild = null;
        for (int i = 0; i < content.childCount; i++)
        {
            var child = content.GetChild(i);
            child.gameObject.SetActive(child.name == type);
            if (child.name == type)
            {
                targetChild = child;
                child.gameObject.SetActive(true);
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }

        // 获取对应类型的所有科技节点
        var techNodes = targetChild.GetComponentsInChildren<UITechNode>();
        foreach (var node in techNodes)
        {
            var data = Resources.Load<ScriptableTechnologyNode>("ScriptableObject/Technology/" + node.name);
            node.DisplayTechNode(data);
            node.onClick.RemoveAllListeners();
            node.onClick.AddListener(() =>
            {
                curSelectedTechNode = data;
                DisplayTechNodeDetails(data);
            });
        }

        // 如果是刷新，继续选中上一个选中的科技节点
        if (isRefresh)
            DisplayTechNodeDetails(curSelectedTechNode);
        else if (curSelectedTechNode == null)
        {
            curSelectedTechNode = Resources.Load<ScriptableTechnologyNode>("ScriptableObject/Technology/" + techNodes[0].name);
            DisplayTechNodeDetails(curSelectedTechNode);
        }

        SelectTechTreeWithTween(type);
    }

    private void SelectTechTreeWithTween(string type)
    {
        // 停止当前动画
        if (curAnim != null && curAnim.IsActive())
        {
            curAnim.Kill();
        }

        Vector2 targetPos = new(menuItemTransforms[type].anchoredPosition.x, selectRect.anchoredPosition.y);

        // 创建动画序列
        curAnim = DOTween.Sequence();

        curAnim.Append(selectRect.DOAnchorPos(targetPos, 0.2f).SetEase(Ease.OutQuad));
    }

    private void RefreshCurrentDisplay()
    {
        DisplayTechTree(curSelectedType, true);
    }

    private void DisplayTechNodeDetails(ScriptableTechnologyNode techNode)
    {
        // 销毁前置研究和解锁配方对应的预制体
        foreach (var obj in temp)
        {
            DestroyImmediate(obj);
        }
        temp.Clear();

        // 显示科技的名称和描述
        techName.text = techNode.techName;
        techDescription.text = techNode.techDescription;

        // 显示科技的前置研究项目
        prerequisite.SetActive(techNode.prerequisites.Count != 0);

        var prerequisitePrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/Study/TechPrerequisite");
        foreach (var prerequisite in techNode.prerequisites)
        {
            var toggle = Instantiate(prerequisitePrefab, detailLayout).GetComponentInChildren<StateToggle>();
            unlockRecipe.transform.SetAsLastSibling();
            toggle.SetStateName(prerequisite.techName);
            toggle.SetValue(TechnologyManager.Instance.IsTechNodeComplished(prerequisite));
            temp.Add(toggle.gameObject);
        }

        // 显示可以解锁的配方
        var recipeItem = Resources.Load<GameObject>("Prefabs/UI/Controls/Study/RecipeItem_Details");
        foreach (var recipe in techNode.recipes)
        {
            var button = Instantiate(recipeItem, detailLayout).GetComponent<HoverableButton>();
            button.normalImage.sprite = recipe.CardImage;
            button.GetComponentsInChildren<Text>()[1].text = recipe.cardId;
            temp.Add(button.gameObject);
        }

        // 显示研究按钮
        studyButton.DisplayButton(techNode, () =>
        {
            // 暂停当前研究
            TechnologyManager.Instance.StopStudy();
            // 研究当前科技节点
            TechnologyManager.Instance.Study(techNode);
            // 刷新显示
            RefreshCurrentDisplay();
        }, () =>
        {
            // 暂停当前研究
            TechnologyManager.Instance.StopStudy();
            // 刷新显示
            RefreshCurrentDisplay();
        });

        // 显示研究进度和研究时间
        // 研究已完成
        if (TechnologyManager.Instance.IsTechNodeComplished(techNode))
        {
            progressSlider.gameObject.SetActive(false);
            studyTime.transform.parent.gameObject.SetActive(false);
            //progressSlider.SetValue(techNode.cost, techNode.cost);
        }
        // 其他情况
        else
        {
            progressSlider.gameObject.SetActive(true);
            var progress = TechnologyManager.Instance.GetStudyProgress(techNode);
            progressSlider.SetValue(progress, techNode.cost);

            // 显示研究时间
            studyTime.transform.parent.gameObject.SetActive(true);
            var time = techNode.cost * 15;
            int hour = time / 60;
            int minute = time % 60;
            StringBuilder sb = new();
            sb.Append(hour > 0 ? $"{hour}h" : "");
            sb.Append(minute > 0 ? $"{minute}min" : "");
            studyTime.text = sb.ToString();
        }

        // 显示研究速度
        if (TechnologyManager.Instance.IsTechNodeBeingStudied(techNode))
        {
            studyRate.gameObject.SetActive(true);
            studyRate.text = $"+{TechnologyManager.Instance.CurStudyRate:0.0}科技点/15min";
        }
        else
        {
            studyRate.gameObject.SetActive(false);
        }
    }
}
