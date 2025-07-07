using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WindowsManager : MonoBehaviour, IPointerDownHandler
{
    private static WindowsManager instance;
    public static WindowsManager Instance => instance;

    [SerializeField] private Desktop desktop; // 桌面布局
    [SerializeField] private BottomBar bottomBar; // 底边栏布局
    [SerializeField] private Button settingsButton; // 设置按钮(Windows键)

    private Dictionary<string, WindowBase> openedWindows = new(); // 当前所有打开的窗口
    private WindowBase currentFocusedWindow; // 当前持有焦点的窗口，可能是openWindows[0]，可能是null

    private WindowGroup windowGroup; // 所有窗口作为其子物体，由该脚本控制窗口的渲染顺序

    private List<App> appsData;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        windowGroup = transform.Find("Desktop/WindowGroup").GetComponent<WindowGroup>();

        currentFocusedWindow = null;

        // 加载桌面图标数据
        appsData = Resources.Load<AppsData>("SO/TestAppsData").appsData;
        desktop.Init(appsData);

        // 添加settingsButton点击的回调
    }

    public void OpenWindow(string appName)
    {
        WindowBase window;
        // 实例化或者直接得到已经打开过的窗口
        if (!IsWindowOpen(appName))
        {
            // 实例化窗口对象
            GameObject windowPrefab = Resources.Load<GameObject>($"Prefabs/UI/Windows/{appName}Window");
            window = Instantiate(windowPrefab, windowGroup.transform).GetComponent<WindowBase>();
            // 添加到已打开窗口中
            openedWindows.Add(appName, window);

            // 将快捷方式添加到底部栏
            bottomBar.AddShortcut(appsData.Find(app => app.name == appName));
        }
        else
        {
            window = openedWindows[appName];
        }

        // 打开窗口
        window.Open();
        // 让窗口获得焦点
        FocusWindow(window);
    }

    public void CloseWindow(string appName)
    {
        if (!IsWindowOpen(appName)) return;

        WindowBase window = openedWindows[appName];
        openedWindows.Remove(appName);
        window.Close();

        // 将底边栏的快捷方式移除
        bottomBar.RemoveShortcut(appName);

        // 设置获得焦点的窗口是渲染层级最靠前的窗口
        // 或者是null
        FocusWindow(windowGroup.GetTheFrontWindow());
    }

    public void MaximizeWindow(string appName)
    {

    }

    public void MinimizeWindow(string appName)
    {

    }

    public void FocusWindow(WindowBase window)
    {
        // 如果当前窗口已经获取焦点，则直接返回
        if (IsWindowFocused(window)) return;

        currentFocusedWindow = window;
        // 设置window的聚焦状态
        foreach (var (name, win) in openedWindows)
        {
            win.SetFocused(win == window);
        }

        // 如果窗口为null，则直接返回
        if (window == null) return;

        // 调整window的渲染顺序为最高
        windowGroup.FocusWindow(currentFocusedWindow);

        // 清除桌面上的选择
        desktop.ClearSelection();
        // 选中底边栏中的快捷方式
        bottomBar.SelectAppShortcut(window.AppName);
    }

    public void FocusWindow(string appName)
    {
        if (!IsWindowOpen(appName)) return;

        FocusWindow(openedWindows[appName]);
    }

    public WindowBase GetCurrentFocusedWindow() => currentFocusedWindow;

    public bool IsWindowOpen(string appName)
    {
        return openedWindows.ContainsKey(appName);
    }

    public bool IsWindowFocused(WindowBase window)
    {
        return currentFocusedWindow == window;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log(eventData.pointerCurrentRaycast.gameObject.name);
        // 根据点击到的内容清除不必要的选择
    }
}