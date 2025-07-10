using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIManager
{
    // UI路径
    private const string UI_PREFABS_PATH = "Prefabs/UI/";

    private static UIManager instance = new UIManager();

    public static UIManager Instance => instance;
    // canvas Transform
    private Transform canvasTransform;

    /// <summary>
    /// 所有面板
    /// </summary>
    private Dictionary<string, PanelBase> panels = new Dictionary<string, PanelBase>();

    private UIManager()
    {
        Init();
    }

    private void Init()
    {
        // 实例化唯一canvas
        InstantiateUniqueCanvas();
    }

    private void InstantiateUniqueCanvas()
    {
        // 查找canvas
        GameObject canvasObject = GameObject.Find("Canvas");
        if (canvasObject == null)
        {
            GameObject canvasPrefab = Resources.Load<GameObject>(UI_PREFABS_PATH + "Canvas");
            canvasObject = Object.Instantiate(canvasPrefab);
        }

        canvasTransform = canvasObject.transform;

        // 不销毁canvas
        Object.DontDestroyOnLoad(canvasObject);
    }

    // 创建面板
    public T CreatePanel<T>() where T : PanelBase
    {
        // 查找面板
        string panelName = typeof(T).Name;
        if (panels.ContainsKey(panelName))
            return panels[panelName] as T; // 返回面板

        // 实例化面板
        GameObject panelPrefab = Resources.Load<GameObject>(UI_PREFABS_PATH + "Panels/" + panelName);
        GameObject panelObject = Object.Instantiate(panelPrefab, canvasTransform);

        PanelBase panel = panelObject.GetComponent<PanelBase>();
        panels.Add(panelName, panel); // 添加面板
        return panel as T;
    }

    /// <summary>
    /// 显示面板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="showMode">显示模式</param>
    /// <param name="onFinished">完成回调</param>
    /// <returns></returns>
    public T ShowPanel<T>(PanelBase.ShowMode showMode = PanelBase.ShowMode.Fade, UnityAction onFinished = null) where T : PanelBase
    {
        string panelName = typeof(T).Name;
        PanelBase panel;
        if (panels.ContainsKey(panelName))
            panel = panels[panelName] as T;
        else
            // 创建面板
            panel = CreatePanel<T>();

            // 显示面板
        panel.Show(showMode, onFinished);

        return panel as T;
    }

    /// <summary>
    /// 隐藏面板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="showMode">显示模式</param>
    /// <param name="onFinished">完成回调</param>
    /// <param name="destroy">是否销毁</param>
    public void HidePanel<T>(PanelBase.ShowMode showMode = PanelBase.ShowMode.Fade, UnityAction onFinished = null, bool destroy = true) where T : PanelBase
    {
        string panelName = typeof(T).Name;
        if (panels.ContainsKey(panelName))
        {
            PanelBase panel = panels[panelName];
            if (destroy)
            {
                onFinished += () => Object.Destroy(panel.gameObject); // 销毁GameObject
                panels.Remove(panelName); // 移除面板
            }

            panel.Hide(showMode, onFinished);
        }
        // 销毁面板
    }

    /// <summary>
    /// 获取面板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetPanel<T>() where T : PanelBase
    {
        string panelName = typeof(T).Name;
        if (panels.ContainsKey(panelName))
            return panels[panelName] as T; // 返回面板
        return null; // 返回null
    }
}
