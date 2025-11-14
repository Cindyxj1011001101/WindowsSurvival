using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public int targetWidth = 1920;
    public int targetHeight = 1080;
    public bool isFullscreen = true; // 根据需求设置全屏还是窗口模式

    public int targetFrameRate = 60;

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
        ElectricPowerManager.Instance,
        Player.Instance,
        StateManager.Instance,
        GameManager.Instance,
        MoveExploreManager.Instance,
    };

    private void Awake()
    {
        // 设置分辨率
        SetResolution();
        //SetFrameRate();

        // 初始化各类管理器
        InitManagers();
    }

    private void OnDestroy()
    {
        foreach (var m in orderedManagers)
        {
            m.Reset();
        }
    }

    private void InitManagers()
    {
        foreach (var m in orderedManagers)
        {
            m.Init();
        }
    }

    private void SetResolution()
    {
        // 检查当前分辨率是否已是目标分辨率，避免不必要的设置（可选）
        if (Screen.currentResolution.width != targetWidth || Screen.currentResolution.height != targetHeight || Screen.fullScreen != isFullscreen)
        {
            // 设置屏幕分辨率
            Screen.SetResolution(targetWidth, targetHeight, isFullscreen);
            // 如果你希望窗口模式，可以将 isFullscreen 设置为 false
            // Screen.SetResolution(targetWidth, targetHeight, FullScreenMode.Windowed);
        }
        // 确保在其他地方没有覆盖此设置（例如，在玩家更改设置后保存并加载他们的偏好）
    }

    private void SetFrameRate()
    {
        Application.targetFrameRate = targetFrameRate;
    }
}
