using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIManager
{
    // UI相关预制体的保存路径
    private const string UI_PREFABS_PATH = "Prefabs/UI/";

    private static UIManager instance = new UIManager();

    public static UIManager Instance => instance;
    // canvas的Transform
    private Transform canvasTransform;

    /// <summary>
    /// 存储场景上所有显示着的面板，方便后续获取
    /// </summary>
    private Dictionary<string, PanelBase> panels = new Dictionary<string, PanelBase>();

    private UIManager()
    {
        Init();
    }

    private void Init()
    {
        // 实例化一个唯一的canvas对象
        InstantiateUniqueCanvas();
    }

    private void InstantiateUniqueCanvas()
    {
        // 取得canvas对象
        GameObject canvasObject = GameObject.Find("Canvas");
        if (canvasObject == null)
        {
            GameObject canvasPrefab = Resources.Load<GameObject>(UI_PREFABS_PATH + "Canvas");
            canvasObject = Object.Instantiate(canvasPrefab);
        }

        canvasTransform = canvasObject.transform;

        // Canvas在过场景时不会被销毁
        // 保证Canvse对象的唯一性
        Object.DontDestroyOnLoad(canvasObject);
    }

    // 创建一个面板，将面板存入字典，不会调用显示方法
    // 用于当面板中有较多资源需要加载时，可以先把面板创建出来，然后异步地加载资源
    // 待资源加载完毕以后再调用显示方法，避免主线程卡顿
    public T CreatePanel<T>() where T : PanelBase
    {
        // 以面板的类名为键存储面板
        string panelName = typeof(T).Name;
        if (panels.ContainsKey(panelName))
            return panels[panelName] as T; // 如果面板已经存在，直接返回

        // 创建面板实例
        // 要保证面板预制体的名称与其类名相同
        GameObject panelPrefab = Resources.Load<GameObject>(UI_PREFABS_PATH + "Panels/" + panelName);
        GameObject panelObject = Object.Instantiate(panelPrefab, canvasTransform);

        PanelBase panel = panelObject.GetComponent<PanelBase>();
        panels.Add(panelName, panel); // 将面板添加到字典中
        return panel as T;
    }

    /// <summary>
    /// 显示面板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="showMode">显示方式</param>
    /// <param name="onFinished">面板完全显示以后执行的逻辑</param>
    /// <returns></returns>
    public T ShowPanel<T>(PanelBase.ShowMode showMode = PanelBase.ShowMode.Fade, UnityAction onFinished = null) where T : PanelBase
    {
        string panelName = typeof(T).Name;
        PanelBase panel;
        if (panels.ContainsKey(panelName))
            panel = panels[panelName] as T;
        else
            // 若面板不存在则先创建
            panel = CreatePanel<T>();

        // 调用面板的显示方法
        panel.Show(showMode, onFinished);

        return panel as T;
    }

    /// <summary>
    /// 隐藏面板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="showMode">隐藏方式</param>
    /// <param name="onFinished">面板完全隐藏以后执行的逻辑</param>
    /// <param name="onFinished">面板完全隐藏以后执行是否销毁面板</param>
    public void HidePanel<T>(PanelBase.ShowMode showMode = PanelBase.ShowMode.Fade, UnityAction onFinished = null, bool destroy = true) where T : PanelBase
    {
        string panelName = typeof(T).Name;
        if (panels.ContainsKey(panelName))
        {
            PanelBase panel = panels[panelName];
            if (destroy)
            {
                onFinished += () => Object.Destroy(panel.gameObject); // 面板隐藏后销毁其GameObject
                panels.Remove(panelName); // 从字典中移除面板
            }

            panel.Hide(showMode, onFinished);
        }
        // 场景中不存在要隐藏的面板则什么也不做
    }

    /// <summary>
    /// 得到面板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetPanel<T>() where T : PanelBase
    {
        string panelName = typeof(T).Name;
        if (panels.ContainsKey(panelName))
            return panels[panelName] as T; // 返回指定类型的面板
        return null; // 如果面板不存在，返回null
    }
}
