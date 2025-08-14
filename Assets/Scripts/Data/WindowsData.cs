using System.Collections.Generic;
using UnityEngine;

public class WindowData
{
    public Vector3 position;
    public Vector3 scale;
    public Vector3 sizeDelta;
    public WindowState lastState;
    public WindowState state;
    public Vector3 lastPosition;
    public Vector3 lastSizeDelta;
    public bool isModal;
}

public class WindowsData
{
    public string focusedWindow = string.Empty;

    public List<string> unlockedShortcuts = new(); // 已解锁的快捷方式

    public Dictionary<string, WindowData> openedWindows = new();
}