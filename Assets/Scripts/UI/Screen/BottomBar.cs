using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BottomBar : MonoBehaviour
{
    private Transform layoutTransform;

    private Dictionary<string, BottomBarShortcut> shortcuts = new();

    public BottomBarShortcut this[string appName]
    {
        get
        {
            if (shortcuts.ContainsKey(appName))
                return shortcuts[appName];
            return null;
        }
    }

    private void Start()
    {
        layoutTransform = GetComponentInChildren<GridLayoutGroup>().transform;
    }
    public void AddShortcut(App app)
    {
        if (shortcuts.ContainsKey(app.name)) return;

        GameObject shortcutPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/BottomBarShortcut");
        BottomBarShortcut shortcut = Instantiate(shortcutPrefab, layoutTransform).GetComponent<BottomBarShortcut>();
        shortcut.Init(app);
        shortcuts.Add(app.name, shortcut);
    }

    public void RemoveShortcut(string appName)
    {
        if (!shortcuts.ContainsKey(appName)) return;

        // 找到要移除的快捷方式
        var toRemove = shortcuts[appName];
        // 调用移除的方法
        toRemove.Disappear();
        shortcuts.Remove(appName);
    }

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
    }


    public void ClearSelection()
    {
        foreach (var shortcut in shortcuts.Values)
        {
            shortcut.SetSelected(false);
        }
    }
}