using UnityEngine;

public class Main : MonoBehaviour
{
    public int targetWidth = 1920;
    public int targetHeight = 1080;
    public bool isFullscreen = true; // 根据需求设置全屏还是窗口模式

    public int targetFrameRate = 60;

    void Start()
    {
        // Debug.Log(Application.persistentDataPath);

        SetResolution();
        //SetFrameRate();
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
