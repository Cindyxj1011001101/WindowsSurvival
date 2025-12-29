using System;
using System.Collections.Generic;

/// <summary>
/// 对话触发条件：打开科技窗口（研究窗口）时触发
/// 检测事件类型："AwakeWindow"，值："Study"
/// </summary>
public class OpenTechnologyWindow : ChatCondition
{
    public OpenTechnologyWindow(string name, bool startedDetect, bool isUnlocked,  Action<List<ChatData>> onUnlocked,ChatData chatData) : base(name, startedDetect, isUnlocked, onUnlocked,chatData)
    {
    }
    public override bool Detect(string type, string value)
    {
        if(type=="AwakeWindow"&&value=="Study")
        {
            return true;
        }
        return false;
    }
}