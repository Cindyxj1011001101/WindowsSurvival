using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIManager
{
    // UI���Ԥ����ı���·��
    private const string UI_PREFABS_PATH = "Prefabs/UI/";

    private static UIManager instance = new UIManager();

    public static UIManager Instance => instance;
    // canvas��Transform
    private Transform canvasTransform;

    /// <summary>
    /// �洢������������ʾ�ŵ���壬���������ȡ
    /// </summary>
    private Dictionary<string, PanelBase> panels = new Dictionary<string, PanelBase>();

    private UIManager()
    {
        Init();
    }

    private void Init()
    {
        // ʵ����һ��Ψһ��canvas����
        InstantiateUniqueCanvas();
    }

    private void InstantiateUniqueCanvas()
    {
        // ȡ��canvas����
        GameObject canvasObject = GameObject.Find("Canvas");
        if (canvasObject == null)
        {
            GameObject canvasPrefab = Resources.Load<GameObject>(UI_PREFABS_PATH + "Canvas");
            canvasObject = Object.Instantiate(canvasPrefab);
        }

        canvasTransform = canvasObject.transform;

        // Canvas�ڹ�����ʱ���ᱻ����
        // ��֤Canvse�����Ψһ��
        Object.DontDestroyOnLoad(canvasObject);
    }

    // ����һ����壬���������ֵ䣬���������ʾ����
    // ���ڵ�������н϶���Դ��Ҫ����ʱ�������Ȱ���崴��������Ȼ���첽�ؼ�����Դ
    // ����Դ��������Ժ��ٵ�����ʾ�������������߳̿���
    public T CreatePanel<T>() where T : PanelBase
    {
        // ����������Ϊ���洢���
        string panelName = typeof(T).Name;
        if (panels.ContainsKey(panelName))
            return panels[panelName] as T; // �������Ѿ����ڣ�ֱ�ӷ���

        // �������ʵ��
        // Ҫ��֤���Ԥ�������������������ͬ
        GameObject panelPrefab = Resources.Load<GameObject>(UI_PREFABS_PATH + "Panels/" + panelName);
        GameObject panelObject = Object.Instantiate(panelPrefab, canvasTransform);

        PanelBase panel = panelObject.GetComponent<PanelBase>();
        panels.Add(panelName, panel); // �������ӵ��ֵ���
        return panel as T;
    }

    /// <summary>
    /// ��ʾ���
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="showMode">��ʾ��ʽ</param>
    /// <param name="onFinished">�����ȫ��ʾ�Ժ�ִ�е��߼�</param>
    /// <returns></returns>
    public T ShowPanel<T>(PanelBase.ShowMode showMode = PanelBase.ShowMode.Fade, UnityAction onFinished = null) where T : PanelBase
    {
        string panelName = typeof(T).Name;
        PanelBase panel;
        if (panels.ContainsKey(panelName))
            panel = panels[panelName] as T;
        else
            // ����岻�������ȴ���
            panel = CreatePanel<T>();

        // ����������ʾ����
        panel.Show(showMode, onFinished);

        return panel as T;
    }

    /// <summary>
    /// �������
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="showMode">���ط�ʽ</param>
    /// <param name="onFinished">�����ȫ�����Ժ�ִ�е��߼�</param>
    /// <param name="onFinished">�����ȫ�����Ժ�ִ���Ƿ��������</param>
    public void HidePanel<T>(PanelBase.ShowMode showMode = PanelBase.ShowMode.Fade, UnityAction onFinished = null, bool destroy = true) where T : PanelBase
    {
        string panelName = typeof(T).Name;
        if (panels.ContainsKey(panelName))
        {
            PanelBase panel = panels[panelName];
            if (destroy)
            {
                onFinished += () => Object.Destroy(panel.gameObject); // ������غ�������GameObject
                panels.Remove(panelName); // ���ֵ����Ƴ����
            }

            panel.Hide(showMode, onFinished);
        }
        // �����в�����Ҫ���ص������ʲôҲ����
    }

    /// <summary>
    /// �õ����
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetPanel<T>() where T : PanelBase
    {
        string panelName = typeof(T).Name;
        if (panels.ContainsKey(panelName))
            return panels[panelName] as T; // ����ָ�����͵����
        return null; // �����岻���ڣ�����null
    }
}
