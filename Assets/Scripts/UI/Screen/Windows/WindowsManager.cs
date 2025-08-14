using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class WindowsManager : MonoBehaviour
{
    private static WindowsManager instance;
    public static WindowsManager Instance => instance;

    [SerializeField] private ShortcutsController shortcutsController; // 管理快捷方式

    [SerializeField] private WindowGroup windowGroup; // 所有窗口作为其子物体，由该脚本控制窗口的渲染顺序

    [SerializeField] private RectTransform hoverTipLayer;
    [SerializeField] private RectTransform tempCardSlotLayer;
    [SerializeField] private RectTransform floatingCardSlotLayer;
    [SerializeField] private RectTransform chatTipLayer;

    public RectTransform HoverTipLayer => hoverTipLayer;
    public RectTransform TempCardSlotLayer => tempCardSlotLayer;
    public RectTransform FloatingTipLayer => floatingCardSlotLayer;
    public RectTransform ChatTipLayer => chatTipLayer;

    private Dictionary<string, WindowBase> openedWindows = new(); // 当前所有打开的窗口，最小化的窗口也算打开的
    private WindowBase currentFocusedWindow; // 当前持有焦点的窗口，可能是openWindows[0]，可能是null

    [SerializeField] private HoverableButton saveButton;
    [SerializeField] private HoverableButton restButton;

    private void Awake()
    {
        instance = this;
        pointerData = new(EventSystem.current)
        {
            // 设置指针位置为鼠标位置
            position = Input.mousePosition
        };
    }

    private void Start()
    {
        saveButton.onClick.AddListener(() =>
        {
            GameDataManager.Instance.SaveAllData();
            SceneManager.LoadScene(0);
        });

        restButton.onClick.AddListener(() =>
        {
            var window = (OpenWindow("TimeSelect", true) as TimeSelectWindow);
            window.onConfirm += (time) =>
            {
                StateManager.Instance.Sleep(time);
            };
            window.getConfirmEffects += (t) =>
            {
                Dictionary<PlayerStateEnum, float> p = null;
                float sobrietyChange = t / TimeManager.Instance.SettleInterval * StateManager.Instance.SobrietyChangeRateWhileSleeping;
                if (sobrietyChange > 0)
                {
                    p = new()
                    {
                        { PlayerStateEnum.Sobriety, sobrietyChange }
                    };
                }
                return ($"休息{t}分钟", t, p, null);
            };
        });
    }

    public WindowBase OpenWindow(string appName, bool isModal = false)
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
                window = Instantiate(windowPrefab, windowGroup.transform).GetComponent<WindowBase>();
                windowGroup.SetClosed(window);
            }
            // 添加到已打开窗口中
            openedWindows.Add(appName, window);
            // 底边栏的快捷方式变亮
            shortcutsController.SetOpened(appName, true);
        }
        else
        {
            window = openedWindows[appName];
        }
        if (window.IsPlayingAnim) return window;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("万能泡泡音", true);

        window.SetModal(isModal);

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

        // 设置为未聚焦
        window.SetFocused(false);

        window.Close();

        // 将窗口从渲染层级中移除
        windowGroup.SetClosed(window);

        // 底边栏的快捷方式变暗
        shortcutsController.SetOpened(appName, false);

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

        if (window.IsPlayingAnim) return;
        // 最小化窗口
        window.Minimize(shortcutsController[appName].transform);

        // 将window暂停渲染
        windowGroup.SetMinimized(window);

        // 设置获得焦点的窗口是渲染层级最靠前的窗口
        // 或者是null
        FocusWindow(windowGroup.GetTheFrontWindow());
    }

    public void FocusWindow(WindowBase window)
    {
        // 如果当前窗口已经获取焦点，则直接返回
        if (IsWindowFocused(window)) return;

        // 设置window的聚焦状态
        if (currentFocusedWindow != null)
            currentFocusedWindow.SetFocused(false);

        if (window != null)
            window.SetFocused(true);

        currentFocusedWindow = window;

        // 调整window的渲染顺序为最高
        windowGroup.SetFocused(currentFocusedWindow);

        if (window != null)
            // 选中底边栏中的快捷方式
            shortcutsController.SelectAppShortcut(window.AppName);
        else
            shortcutsController.ClearSelection();
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

    public void UnlockShortcut(string appName, bool blink = true)
    {
        shortcutsController.SetLocked(appName, false, blink);
    }

    public List<string> GetUnlockedShortcuts()
    {
        return shortcutsController.GetUnlockedShortcuts();
    }


    PointerEventData pointerData;
    List<RaycastResult> results;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 检测鼠标左键点击
        {
            // 创建接收结果的列表
            results = new List<RaycastResult>();

            // 执行射线检测
            EventSystem.current.RaycastAll(pointerData, results);

            // 处理检测结果
            if (results.Count > 0)
            {
                foreach (var result in results)
                {
                    if (result.gameObject.TryGetComponent<WindowBase>(out var window))
                    {
                        FocusWindow(window);
                        return;
                    }
                }
            }
        }
    }
}