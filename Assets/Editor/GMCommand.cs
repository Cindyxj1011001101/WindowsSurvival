using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GMCommand
{
    [MenuItem("Command/�򿪵ױ���")]
    public static void ShowBottomBarPanel()
    {
        UIManager.Instance.ShowPanel<BottomBarPanel>();
    }

    [MenuItem("Command/���������ݷ�ʽ")]
    public static void LoadDesktopShortcuts()
    {
        // ��������
        List<App> appsData = Resources.Load<AppsData>("SO/TestAppsData").appsData;
        foreach (App app in appsData)
        {
            Debug.Log(app);
        }
    }

    [MenuItem("Command/��ʾ�����ݷ�ʽ")]
    public static void ShowDesktopShortcuts()
    {
        // ��������
        List<App> appsData = Resources.Load<AppsData>("SO/TestAppsData").appsData;
        Object.FindObjectOfType<Desktop>().Init(appsData);
    }

    [MenuItem("Command/�򿪱�������")]
    public static void OpenBackpackWindow()
    {
        WindowsManager.Instance.OpenWindow("Backpack");
    }

    [MenuItem("Command/�رձ�������")]
    public static void CloseBackpackWindow()
    {
        WindowsManager.Instance.CloseWindow("Backpack");
    }
}
