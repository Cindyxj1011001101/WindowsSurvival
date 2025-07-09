using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Desktop : MonoBehaviour
{
    private Transform layoutTransform;

    private Dictionary<string, DesktopShortcut> shortcuts = new();

    private void Awake()
    {
        layoutTransform = GetComponentInChildren<GridLayoutGroup>().transform;
    }

    public void Init(List<App> appsData)
    {
        foreach (var app in appsData)
        {
            if (app.displayOnDesktop)
                AddShortcut(app);
        }
    }

    public void AddShortcut(App app)
    {
        if (shortcuts.ContainsKey(app.name)) return;

        GameObject shortcutPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/DesktopShortcut");
        DesktopShortcut shortcut = Instantiate(shortcutPrefab, layoutTransform).GetComponent<DesktopShortcut>();
        shortcut.Init(app);
        shortcuts.Add(app.name, shortcut);
    }

    public void SelectAppShortcut(string appName)
    {
        foreach (var shortcut in shortcuts.Values)
        {
            shortcut.SetSelected(appName == shortcut.AppName);
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