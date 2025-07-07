using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GMCommand
{
    [MenuItem("Command/打开底边栏")]
    public static void ShowBottomBarPanel()
    {
        UIManager.Instance.ShowPanel<BottomBarPanel>();
    }

    [MenuItem("Command/加载桌面快捷方式")]
    public static void LoadDesktopShortcuts()
    {
        // 加载数据
        List<App> appsData = Resources.Load<AppsData>("SO/TestAppsData").appsData;
        foreach (App app in appsData)
        {
            Debug.Log(app);
        }
    }

    [MenuItem("Command/显示桌面快捷方式")]
    public static void ShowDesktopShortcuts()
    {
        // 加载数据
        List<App> appsData = Resources.Load<AppsData>("SO/TestAppsData").appsData;
        Object.FindObjectOfType<Desktop>().Init(appsData);
    }

    [MenuItem("Command/打开背包窗口")]
    public static void OpenBackpackWindow()
    {
        WindowsManager.Instance.OpenWindow("Backpack");
    }

    [MenuItem("Command/关闭背包窗口")]
    public static void CloseBackpackWindow()
    {
        WindowsManager.Instance.CloseWindow("Backpack");
    }
}
