using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BottomBar : MonoBehaviour
{
    private Transform layoutTransform;
    [SerializeField] RectTransform selectRect;

    private Dictionary<string, HoverableButton> shortcuts = new();

    private string selectedAppName;
    private Sequence currentAnimation;

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
    }

    public void SelectAppShortcut(string appName)
    {
        if (selectedAppName == appName) return;

        // 停止当前动画
        if (currentAnimation != null && currentAnimation.IsActive())
        {
            currentAnimation.Kill();
        }

        Vector2 startPos = string.IsNullOrEmpty(selectedAppName) ?
            selectRect.anchoredPosition :
            (shortcuts[selectedAppName].transform as RectTransform).anchoredPosition;

        selectRect.anchoredPosition = startPos;

        Vector2 targetPos = (shortcuts[appName].transform as RectTransform).anchoredPosition;

        // 显示选中框
        selectRect.gameObject.SetActive(true);

        // 创建动画序列
        currentAnimation = DOTween.Sequence();

        currentAnimation.Append(selectRect.DOAnchorPos(targetPos, 0.2f).SetEase(Ease.OutBack));

        selectedAppName = appName;
    }

    public void ClearSelection()
    {
        selectedAppName = null;
        selectRect.gameObject.SetActive(false);
    }

    public void SetOpened(string appName, bool value)
    {
        SetOpened(shortcuts[appName], value);
    }

    private void SetOpened(HoverableButton shortcut, bool value)
    {
        shortcut.currentColor = value ? ColorManager.white : ColorManager.darkGrey;
        if (!value)
            shortcut.ChangeColor(ColorManager.darkGrey);
    }
}