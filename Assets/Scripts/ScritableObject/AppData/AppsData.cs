using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 应用的静态数据
/// </summary>
[Serializable]
public class App
{
    public string name; // 应用名称(英文)，并且是唯一的
    public string displayText; // 在桌面上显示的文本，中英文皆可
    public Sprite icon; // 应用图标
    public bool displayOnDesktop; // 是否显示在桌面上

    public override string ToString()
    {
        return $"name: {name}, displayText: {displayText}, icon: {icon}";
    }
}

[CreateAssetMenu(fileName = "New Apps Data", menuName = "ScriptableObject/AppsData", order = 1)]
public class AppsData : ScriptableObject
{
    public List<App> appsData;
}