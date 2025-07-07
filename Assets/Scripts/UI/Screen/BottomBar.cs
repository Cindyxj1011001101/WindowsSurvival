using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BottomBar : MonoBehaviour
{
    private Transform layoutTransform;

    private List<BottomBarShortcut> shortcuts = new List<BottomBarShortcut>();

    private void Start()
    {
        layoutTransform = GetComponentInChildren<GridLayoutGroup>().transform;
    }
    public void AddShortcut(App app)
    {
        if (ShortcutExists(app.name)) return;

        GameObject shortcutPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/BottomBarShortcut");
        BottomBarShortcut shortcut = Instantiate(shortcutPrefab, layoutTransform).GetComponent<BottomBarShortcut>();
        shortcut.Init(app, this);
        shortcuts.Add(shortcut);
    }

    public void RemoveShortcut(string appName)
    {
        if (!ShortcutExists(appName)) return;

        // 找到要移除的快捷方式
        var toRemove = shortcuts.Find(x => x.AppName == appName);
        // 调用移除的方法
        toRemove.Disappear();
        shortcuts.Remove(toRemove);
    }

    public void SelectAppShortcut(string appName)
    {
        foreach (var shortcut in shortcuts)
        {
            // 找到被选中的对象
            if (appName == shortcut.AppName)
            {
                // 如果该对象已经被选中
                if (shortcut.Selected)
                {
                    // 则取消选中
                    shortcut.SetSelected(false);
                }
                else
                {
                    shortcut.SetSelected(true);
                }
            }
            // 其他没有被选中的对象都是false
            else
            {
                shortcut.SetSelected(false);
            }
        }
    }

    private bool ShortcutExists(string appName)
    {
        return shortcuts.Find(shortcut => shortcut.AppName == appName);
    }
}