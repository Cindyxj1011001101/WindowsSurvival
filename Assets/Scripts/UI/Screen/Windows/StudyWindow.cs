using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StudyWindow : WindowBase
{
    [SerializeField] private Text techName;
    [SerializeField] private Text techDescription;

    [SerializeField] private StudyButton studyButton;
    [SerializeField] private UIStateSlider progressSlider;
    [SerializeField] private Text studyInfo;
    [SerializeField] private Text studyTime;

    [SerializeField] private Transform detailLayout;
    [SerializeField] private Transform menuLayout;
    [SerializeField] private Transform content;

    [SerializeField] private GameObject prerequisite;
    [SerializeField] private GameObject unlockRecipe;

    [SerializeField] private RectTransform selectRect;

    [SerializeField] private GameObject recipeItem;
    [SerializeField] private GameObject prerequisitePrefab;

    private int studyState;
    [SerializeField] private HoverableButton studyStateButton; // 显示研究状态的按钮
    [SerializeField] private Animator studyStateButtonAnimator;

    private ScriptableTechnologyNode curSelectedTechNode; // 记录当前选中的科技节点

    private Dictionary<TechType, RectTransform> menuItemTransforms = new();

    protected override void Awake()
    {
        base.Awake();
        EventManager.Instance.AddListener(EventType.ChangeStudyProgress, RefreshDisplay);
        EventManager.Instance.AddListener<ScriptableTechnologyNode>(EventType.StudyComplished, OnStudiedComplished);
        EventManager.Instance.AddListener<ScriptableTechnologyNode>(EventType.StudyStarted, OnStudyStarted);
        EventManager.Instance.AddListener(EventType.StudyStopped, OnStudyStopped);
        EventManager.Instance.AddListener(EventType.LockUnlockIntermediateTechnologies, RefreshDisplay);
        EventManager.Instance.AddListener<string>(EventType.StudyInterrupted, OnStudyInterrupted);

        LayoutRebuilder.ForceRebuildLayoutImmediate(menuLayout as RectTransform);

        menuItemTransforms.Clear();
        for (int i = 0; i < menuLayout.childCount; i++)
        {
            var child = menuLayout.GetChild(i);
            var button = child.GetComponent<HoverableButton>();
            var type = (TechType)Enum.Parse(typeof(TechType), child.name);
            button.onClick.AddListener(() =>
            {
                curSelectedTechNode = null;
                DisplayTechTree(type);
            });
            menuItemTransforms.Add(type, child as RectTransform);
        }
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EventType.ChangeStudyProgress, RefreshDisplay);
        EventManager.Instance.RemoveListener<ScriptableTechnologyNode>(EventType.StudyComplished, OnStudiedComplished);
        EventManager.Instance.RemoveListener<ScriptableTechnologyNode>(EventType.StudyStarted, OnStudyStarted);
        EventManager.Instance.RemoveListener(EventType.StudyStopped, OnStudyStopped);
        EventManager.Instance.RemoveListener(EventType.LockUnlockIntermediateTechnologies, RefreshDisplay);
        EventManager.Instance.RemoveListener<string>(EventType.StudyInterrupted, OnStudyInterrupted);
    }

    private void OnStudyStarted(ScriptableTechnologyNode techNode)
    {
        DisplayStudyState(1, techNode);
    }

    private void OnStudiedComplished(ScriptableTechnologyNode techNode)
    {
        curSelectedTechNode = techNode;
        RefreshDisplay();

        DisplayStudyState(0, techNode);

        studyStateButton.transform.ShowTip($"\"{techNode.techName}\"研究完成！", 1.4f);
    }

    private void OnStudyStopped()
    {
        RefreshDisplay();

        DisplayStudyState(2, null);
    }

    private void OnStudyInterrupted(string reason)
    {
        if (string.IsNullOrEmpty(reason)) return;

        studyStateButton.transform.ShowTip($"由于{reason}，研究中止！", 1.4f);
        SoundManager.Instance.PlaySound("错误提示");
    }

    protected override void Init()
    {
        if (GameDataManager.Instance.CurLoad.skipGuide || GameDataManager.Instance.WindowsData.unlockedShortcuts.Contains(AppName))
            DisplayStudyState(2, null);
        else
            studyStateButton.SetVisiable(false);

        //TechnologyManager.Instance.InitFromGameData();
    }

    public override void Show(ShowMode showMode = ShowMode.Fade, UnityAction onFinished = null)
    {
        base.Show(showMode, onFinished);

        // 如果没有当前选择的节点，则尝试选择正在研究的节点
        if (curSelectedTechNode == null)
            curSelectedTechNode = TechnologyManager.Instance.CurStudiedTechNode;

        DisplayTechTree(curSelectedTechNode == null ? 0 : curSelectedTechNode.techType);
    }

    public override void Hide(ShowMode showMode = ShowMode.Fade, UnityAction onFinished = null)
    {
        base.Hide(showMode, onFinished);
        curSelectedTechNode = null;
    }

    private void DisplayTechTree(TechType type)
    {
        // 只显示对应类型的科技节点
        Transform targetChild = null;
        for (int i = 0; i < content.childCount; i++)
        {
            var child = content.GetChild(i);
            if (child.name == type.ToString())
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
            var data = Resources.Load<ScriptableTechnologyNode>($"ScriptableObject/Technology/{type}/{node.name}");
            node.DisplayTechNode(data);
            node.onClick.RemoveAllListeners();
            node.onClick.AddListener(() =>
            {
                curSelectedTechNode = data;
                DisplayTechNodeDetails(data);
            });
        }

        if (curSelectedTechNode == null)
        {
            curSelectedTechNode = Resources.Load<ScriptableTechnologyNode>($"ScriptableObject/Technology/{type}/{techNodes[0].name}");
        }

        DisplayTechNodeDetails(curSelectedTechNode);

        SelectTechTreeWithTween(type);
    }

    private void SelectTechTreeWithTween(TechType type)
    {
        Vector2 targetPos = new(menuItemTransforms[type].anchoredPosition.x, selectRect.anchoredPosition.y);

        selectRect.DOKill();
        selectRect.DOAnchorPos(targetPos, 0.2f).SetEase(Ease.OutQuad);
    }

    public void RefreshDisplay()
    {
        if (curSelectedTechNode != null)
            DisplayTechTree(curSelectedTechNode.techType);
    }

    private List<GameObject> temp = new();

    private void DisplayTechNodeDetails(ScriptableTechnologyNode techNode)
    {
        // 销毁前置研究和解锁配方对应的预制体
        foreach (var obj in temp)
        {
            //DestroyImmediate(obj);
            ObjectBufferPool.Instance.Restore(obj);
        }
        temp.Clear();

        // 显示科技的名称和描述
        techName.text = techNode.techName;
        techDescription.text = techNode.techDescription;

        // 显示科技的前置研究项目
        prerequisite.SetActive(techNode.prerequisites.Count != 0);

        UIStateToggle toggle;
        foreach (var prerequisite in techNode.prerequisites)
        {
            toggle = ObjectBufferPool.Instance.Get(prerequisitePrefab, detailLayout).GetComponentInChildren<UIStateToggle>();
            unlockRecipe.transform.SetAsLastSibling();
            toggle.SetStateName(prerequisite.techName);
            toggle.SetValue(TechnologyManager.Instance.IsTechNodeComplished(prerequisite));
            temp.Add(toggle.gameObject);

            //toggle.transform.SetAsLastSibling();
        }

        HoverableButton button;
        // 显示可以解锁的配方
        foreach (var recipe in techNode.recipes)
        {
            button = ObjectBufferPool.Instance.Get(recipeItem, detailLayout).GetComponent<HoverableButton>();
            button.normalImage.sprite = recipe.CardImage;
            button.GetComponentsInChildren<Text>()[1].text = recipe.cardId;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                (WindowsManager.Instance.OpenWindow("Details") as DetailsWindow).Display(recipe.CardInstance, DisplayType.DetailsAndCraftButton);
            });

            temp.Add(button.gameObject);

            //button.transform.SetAsLastSibling();
        }

        // 显示研究按钮
        studyButton.DisplayButton(techNode, () =>
        {
            // 暂停当前研究
            DisplayStudyState(2, null);
            TechnologyManager.Instance.StopStudy();
            // 研究当前科技节点
            TechnologyManager.Instance.Study(techNode);
            // 刷新显示
            RefreshDisplay();
        }, () =>
        {
            // 暂停当前研究
            DisplayStudyState(2, null);
            TechnologyManager.Instance.StopStudy();
            // 刷新显示
            RefreshDisplay();
        });

        // 显示研究进度和研究时间
        // 研究已完成
        if (TechnologyManager.Instance.IsTechNodeComplished(techNode))
        {
            progressSlider.gameObject.SetActive(false);
            studyTime.transform.parent.gameObject.SetActive(false);
        }
        // 其他情况
        else
        {
            progressSlider.gameObject.SetActive(true);
            var progress = TechnologyManager.Instance.GetStudyProgress(techNode);
            progressSlider.SetValue(progress, techNode.cost);

            // 显示研究时间
            studyTime.transform.parent.gameObject.SetActive(true);
            var leftProgress = techNode.cost - TechnologyManager.Instance.GetStudyProgress(techNode);
            var time = Mathf.CeilToInt(leftProgress * 15f / TechnologyManager.Instance.CurStudyRate);
            int hour = time / 60;
            int minute = time % 60;
            StringBuilder sb = new();
            sb.Append(hour > 0 ? $"{hour}h" : "");
            sb.Append(minute > 0 ? $"{minute}min" : "");
            studyTime.text = sb.ToString();
        }

        // 正在研究中，显示研究速度
        if (TechnologyManager.Instance.IsTechNodeBeingStudied(techNode))
        {
            studyInfo.gameObject.SetActive(true);
            studyInfo.text = $"+{TechnologyManager.Instance.CurStudyRate:0.0}科技点/15min";
        }
        // 未解锁的研究，显示原因
        else if (TechnologyManager.Instance.IsTechNodeLocked(techNode, out string reason))
        {
            studyInfo.gameObject.SetActive(true);
            studyInfo.text = reason;
        }
        else
        {
            studyInfo.gameObject.SetActive(false);
        }
    }

    private void DisplayStudyState(int state, ScriptableTechnologyNode techNode)
    {
        if (TechnologyManager.Instance.AllTechnologiesStudied)
        {
            studyStateButton.SetVisiable(false);
            return;
        }

        if (studyState == state) return;

        studyStateButton.SetVisiable(true);

        studyStateButton.onClick.RemoveAllListeners();
        studyStateButton.onClick.AddListener(() =>
        {
            WindowsManager.Instance.OpenWindow("Study");
        });


        studyStateButton.transform.GetChild(2).gameObject.SetActive(state == 2);
        studyStateButton.transform.GetChild(1).gameObject.SetActive(state != 2);

        string text = "";
        Color color = ColorManager.White;
        switch (state)
        {
            // 研究完成
            case 0:
                text = "研究完成";
                color = ColorManager.Cyan;
                studyStateButtonAnimator.ResetTrigger("Play");
                studyStateButtonAnimator.SetTrigger("Stop");
                break;
            // 开始研究
            case 1:
                text = "正在研究";
                color = ColorManager.White;
                studyStateButtonAnimator.ResetTrigger("Stop");
                studyStateButtonAnimator.SetTrigger("Play");
                studyStateButton.onClick.AddListener(() =>
                {
                    curSelectedTechNode = techNode;
                    RefreshDisplay();
                });
                break;
            // 未在研究
            case 2:
                text = "未在研究";
                color = ColorManager.Red;
                break;
        }

        studyStateButton.GetComponentInChildren<Text>().text = text;
        studyStateButton.hoveredColor = studyStateButton.currentColor = color;
        studyStateButton.ChangeColor(color);

        studyState = state;
    }

    protected override void OnFocused()
    {
        if (studyState == 0)
            // 显示未在研究
            DisplayStudyState(2, null);
    }
}
