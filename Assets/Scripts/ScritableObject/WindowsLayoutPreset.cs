using System;
using UnityEngine;

[Serializable]
public class PositionAndSizeDelta
{
    public Vector3 position;
    public Vector3 sizeDelta;
}

[CreateAssetMenu(fileName = "WindowsLayoutPreset", menuName = "ScriptableObject/WindowsLayoutPreset")]
public class WindowsLayoutPreset : ScriptableObject
{
    public PositionAndSizeDelta cameraWindow;
    public PositionAndSizeDelta chatWindow;
    public PositionAndSizeDelta craftWindow;
    public PositionAndSizeDelta detailsWindow;
    public PositionAndSizeDelta envBagWindow;
    public PositionAndSizeDelta playerBagWindow;
    public PositionAndSizeDelta equipmentWindow;
    public PositionAndSizeDelta stateWindow;
}