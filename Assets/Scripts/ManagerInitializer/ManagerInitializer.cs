using System.Collections.Generic;
using UnityEngine;

public class ManagerInitializer : MonoBehaviour
{
    private List<IManager> orderedManagers = new()
    {
        // 按顺序列出所有需要初始化的Manager
        UpdateManager.Instance,
        GlobalDataManager.Instance,
        TimeManager.Instance,
        SunlightManager.Instance,
        TechnologyManager.Instance,
        CraftManager.Instance,
        GameEventManager.Instance,
        Player.Instance,
        StateManager.Instance,
        GameManager.Instance,
        MoveExploreManager.Instance,
    };

    private void Awake()
    {
        // 初始化各类管理器
        InitManagers();
    }

    private void InitManagers()
    {
        foreach (var m in orderedManagers)
        {
            m.Init();
        }
    }

    private void OnDestroy()
    {
        foreach (var m in orderedManagers)
        {
            m.Reset();
        }
    }
}