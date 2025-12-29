using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对话触发条件：打开相机窗口时触发
/// 检测事件类型："AwakeWindow"，值："Camera"
/// </summary>
public class OpenCameraWindow : ChatCondition
{
    public OpenCameraWindow(string name, bool startedDetect, bool isUnlocked,  Action<List<ChatData>> onUnlocked,ChatData chatData) : base(name, startedDetect, isUnlocked, onUnlocked,chatData)
    {
    }

    public override bool Detect(string type, string value)
    {
        if(type=="AwakeWindow"&&value=="Camera")
        {
            return true;
        }
        return false;
    }
}