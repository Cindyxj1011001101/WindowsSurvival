using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BottomBar : MonoBehaviour
{
    private Transform layoutTransform;
    [SerializeField] RectTransform selectRect;

    private Dictionary<string, BottomBarShortcut> shortcuts = new();

    private string selectedAppName;
    private Sequence currentAnimation;

    public BottomBarShortcut this[string appName]
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
                shortcuts.Add(shortcut.AppName, shortcut);
                shortcut.SetOpened(false);
            }
        }
        selectRect.gameObject.SetActive(false);
    }

    //public void AddShortcut(App app)
    //{
    //    if (shortcuts.ContainsKey(app.name)) return;

    //    GameObject shortcutPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/BottomBarShortcut");
    //    BottomBarShortcut shortcut = Instantiate(shortcutPrefab, layoutTransform).GetComponent<BottomBarShortcut>();
    //    //shortcut.Init(app);
    //    shortcuts.Add(app.name, shortcut);
    //}

    //public void RemoveShortcut(string appName)
    //{
    //    if (!shortcuts.ContainsKey(appName)) return;

    //    // 找到要移除的快捷方式
    //    var toRemove = shortcuts[appName];
    //    // 调用移除的方法
    //    //toRemove.Disappear();
    //    shortcuts.Remove(appName);
    //}

    public void SelectAppShortcut(string appName)
    {
        foreach (var shortcut in shortcuts.Values)
        {
            // 找到被选中的对象
            if (appName == shortcut.AppName)
                // 将选择反过来
                shortcut.SetSelected(!shortcut.Selected);
            // 其他没有被选中的对象都是false
            else
                shortcut.SetSelected(false);
        }
        SelectIconWithTween(appName);
    }


    public void ClearSelection()
    {
        foreach (var shortcut in shortcuts.Values)
        {
            shortcut.SetSelected(false);
        }
        selectedAppName = null;
        selectRect.gameObject.SetActive(false);
    }

    public void SelectIconWithTween(string appName)
    {
        if (selectedAppName == appName) return;

        // 停止当前动画
        if (currentAnimation != null && currentAnimation.IsActive())
        {
            currentAnimation.Kill();
        }

        Vector2 startPos = string.IsNullOrEmpty(selectedAppName) ?
            selectRect.anchoredPosition :
            shortcuts[selectedAppName].RectTransform.anchoredPosition;

        selectRect.anchoredPosition = startPos;

        Vector2 targetPos = shortcuts[appName].RectTransform.anchoredPosition;
        Vector2 targetSize = selectRect.sizeDelta;
        Vector2 stretchedSize = new Vector2(
            targetSize.x * 1.2f, // 拉伸比例
            targetSize.y);

        // 显示选中框
        selectRect.gameObject.SetActive(true);

        // 创建动画序列
        currentAnimation = DOTween.Sequence();

        // 第一步：移动到目标位置并拉伸
        currentAnimation.Append(selectRect.DOAnchorPos(targetPos, 0.2f).SetEase(Ease.OutQuad));
        currentAnimation.Join(selectRect.DOSizeDelta(stretchedSize, 0.2f).SetEase(Ease.OutQuad));

        // 第二步：收缩回正常大小
        currentAnimation.Append(selectRect.DOSizeDelta(targetSize, 0.15f).SetEase(Ease.OutBack));

        //currentSelected = index;
        selectedAppName = appName;
    }

    public void AlignCenters(RectTransform source, RectTransform target)
    {
        // 获取目标中心在世界空间的位置
        Vector3 targetWorldPosition = target.position;

        // 转换到源对象的局部空间
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            source.parent as RectTransform,
            RectTransformUtility.WorldToScreenPoint(null, targetWorldPosition),
            null,
            out Vector2 localPosition))
        {
            source.anchoredPosition = localPosition;
        }
    }

    public void SetOpened(string appName, bool value)
    {
        shortcuts[appName].SetOpened(value);
    }
}