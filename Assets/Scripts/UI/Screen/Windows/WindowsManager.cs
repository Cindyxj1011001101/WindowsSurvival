using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WindowsManager : MonoBehaviour, IPointerDownHandler
{
    private static WindowsManager instance;
    public static WindowsManager Instance => instance;

    //private Desktop desktop; // 桌面布局
    private BottomBar bottomBar; // 底边栏布局

    private Dictionary<string, WindowBase> openedWindows = new(); // 当前所有打开的窗口，最小化的窗口也算打开的
    private WindowBase currentFocusedWindow; // 当前持有焦点的窗口，可能是openWindows[0]，可能是null

    private WindowGroup windowGroup; // 所有窗口作为其子物体，由该脚本控制窗口的渲染顺序

    //private List<App> appsData;

    //public Desktop Desktop => desktop;
    //public BottomBar BottomBar => bottomBar;

    private void Awake()
    {
        instance = this;
        //desktop = transform.Find("Desktop").GetComponent<Desktop>();
        bottomBar = transform.Find("BottomBar").GetComponent<BottomBar>();
        windowGroup = transform.Find("Desktop/WindowGroup").GetComponent<WindowGroup>();
    }

    private void Start()
    {
        currentFocusedWindow = null;

        // 加载桌面图标数据
        //appsData = Resources.Load<AppsData>("ScriptableObject/App/TestAppsData").appsData;
        //desktop.Init(appsData);

    }

    public WindowBase OpenWindow(string appName)
    {
        WindowBase window;
        // 窗口没有打开
        if (!IsWindowOpen(appName))
        {
            // 如果窗口不在closedGroup里，则创建实例
            if (!windowGroup.TeyGetWindowInClosedGroup(appName, out window))
            {
                // 实例化窗口对象
                GameObject windowPrefab = Resources.Load<GameObject>($"Prefabs/UI/Windows/{appName}Window");
                window = Instantiate(windowPrefab, windowGroup.Closed).GetComponent<WindowBase>();
            }
            // 添加到已打开窗口中
            openedWindows.Add(appName, window);
            // 底边栏的快捷方式变亮
            bottomBar.SetOpened(appName, true);
        }
        else
        {
            window = openedWindows[appName];
        }

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("万能泡泡音", true);

        // 打开窗口
        window.Open();
        // 让窗口获得焦点
        FocusWindow(window);
        return window;
    }

    public void CloseWindow(string appName)
    {
        // 窗口必须已经打开
        if (!IsWindowOpen(appName)) return;

        WindowBase window = openedWindows[appName];
        openedWindows.Remove(appName);
        window.Close();

        // 将窗口从渲染层级中移除
        windowGroup.CloseWindow(window);

        // 底边栏的快捷方式变暗
        bottomBar.SetOpened(appName, false);

        // 设置获得焦点的窗口是渲染层级最靠前的窗口
        // 或者是null
        FocusWindow(windowGroup.GetTheFrontWindow());
    }

    //public void MaximizeWindow(string appName)
    //{
    //    if (!IsWindowOpen(appName)) return;

    //    WindowBase window = openedWindows[appName];
    //    // 最大化窗口
    //    window.Maximize();

    //    // 让窗口获得焦点
    //    FocusWindow(window);
    //}

    public void MinimizeWindow(string appName)
    {
        if (!IsWindowOpen(appName)) return;

        WindowBase window = openedWindows[appName];
        // 最小化窗口
        window.Minimize(bottomBar[appName].transform);

        // 将window暂停渲染
        windowGroup.MinimizeWindow(window);

        // 设置获得焦点的窗口是渲染层级最靠前的窗口
        // 或者是null
        FocusWindow(windowGroup.GetTheFrontWindow());
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

        // 调整window的渲染顺序为最高
        windowGroup.FocusWindow(currentFocusedWindow);

        if (window != null)
            // 选中底边栏中的快捷方式
            bottomBar.SelectAppShortcut(window.AppName);
        else
            bottomBar.ClearSelection();
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
        //Debug.Log(eventData.pointerCurrentRaycast.gameObject.name);
        // 根据点击到的内容清除不必要的选择
    }
}