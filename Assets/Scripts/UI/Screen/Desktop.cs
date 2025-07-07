using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Desktop : MonoBehaviour
{
    private Transform layoutTransform;

    private List<DesktopShortcut> shortcuts = new List<DesktopShortcut>();

    private void Start()
    {
        layoutTransform = GetComponentInChildren<GridLayoutGroup>().transform;
    }

    public void Init(List<App> appsData)
    {
        foreach (var app in appsData)
        {
            AddShortcut(app);
        }
    }

    public void AddShortcut(App app)
    {
        if (ShortcutExists(app.name)) return;

        GameObject shortcutPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/DesktopShortcut");
        DesktopShortcut shortcut = Instantiate(shortcutPrefab, layoutTransform).GetComponent<DesktopShortcut>();
        shortcut.Init(app, this);
        shortcuts.Add(shortcut);
    }

    public void SelectAppShortcut(string appName)
    {
        foreach (var shortcut in shortcuts)
        {
            shortcut.SetSelected(appName == shortcut.AppName);
        }
    }

    public void ClearSelection()
    {
        foreach (var shortcut in shortcuts)
        {
            shortcut.SetSelected(false);
        }
    }

    private bool ShortcutExists(string appName)
    {
        return shortcuts.Find(shortcut => shortcut.AppName == appName);
    }
}