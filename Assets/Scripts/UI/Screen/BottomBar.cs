using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BottomBar : MonoBehaviour
{
    private Transform layoutTransform;
    [SerializeField] RectTransform selectRect;

    private Dictionary<string, HoverableButton> shortcuts = new();

    private string selectedAppName;

    [SerializeField] private HoverableButton studyComplishedButton; // 研究完成以后显示的按钮

    public HoverableButton this[string appName]
    {
        get
        {
            if (shortcuts.ContainsKey(appName))
                return shortcuts[appName];
            return null;
        }
    }

    private void Awake()
    {
        layoutTransform = GetComponentInChildren<GridLayoutGroup>().transform;
        for (int i = 0; i < layoutTransform.childCount; i++)
        {
            if (layoutTransform.GetChild(i).TryGetComponent<BottomBarShortcut>(out var shortcut))
            {
                shortcuts.Add(shortcut.name, shortcut);
                SetOpened(shortcut, false);
                shortcut.onClick.AddListener(() =>
                {
                    if (shortcut.name != selectedAppName)
                        WindowsManager.Instance.OpenWindow(shortcut.name);
                    else
                        WindowsManager.Instance.MinimizeWindow(shortcut.name);
                });
            }
        }
        selectRect.gameObject.SetActive(false);

        // 监听研究完成事件
        studyComplishedButton.hoveredColor = studyComplishedButton.currentColor = ColorManager.cyan;
        studyComplishedButton.ChangeColor(ColorManager.cyan);
        EventManager.Instance.AddListener<ScriptableTechnologyNode>(EventType.StudyComplished, OnStudyComplished);
    }

    private void Start()
    {
        studyComplishedButton.SetVisiable(false);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<ScriptableTechnologyNode>(EventType.StudyComplished, OnStudyComplished);
    }

    private void OnStudyComplished(ScriptableTechnologyNode techNode)
    {
        studyComplishedButton.SetVisiable(true);
        studyComplishedButton.onClick.RemoveAllListeners();
        studyComplishedButton.onClick.AddListener(() =>
        {
            WindowsManager.Instance.OpenWindow("Study");
            studyComplishedButton.SetVisiable(false);
        });
    }

    public void SelectAppShortcut(string appName)
    {
        if (selectedAppName == appName) return;

        if (!shortcuts.ContainsKey(appName)) return;

        Vector2 startPos = string.IsNullOrEmpty(selectedAppName) ?
            selectRect.anchoredPosition :
            (shortcuts[selectedAppName].transform as RectTransform).anchoredPosition;

        selectRect.anchoredPosition = startPos;

        Vector2 targetPos = (shortcuts[appName].transform as RectTransform).anchoredPosition;

        // 显示选中框
        selectRect.gameObject.SetActive(true);

        // 创建动画序列
        selectRect.DOKill();
        selectRect.DOAnchorPos(targetPos, 0.2f).SetEase(Ease.OutBack);

        selectedAppName = appName;
    }

    public void ClearSelection()
    {
        selectedAppName = null;
        selectRect.gameObject.SetActive(false);
    }

    public void SetOpened(string appName, bool value)
    {
        if (shortcuts.ContainsKey(appName))
            SetOpened(shortcuts[appName], value);
    }

    private void SetOpened(HoverableButton shortcut, bool value)
    {
        var color = value? ColorManager.white: ColorManager.darkGrey;
        shortcut.currentColor = color;
        shortcut.ChangeColor(color);
    }
}