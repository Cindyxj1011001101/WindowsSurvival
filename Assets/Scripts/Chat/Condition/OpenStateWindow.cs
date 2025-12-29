using System;
using System.Collections.Generic;

/// <summary>
/// 对话触发条件：打开状态窗口时触发
/// 检测事件类型："AwakeWindow"，值："State"
/// </summary>
public class OpenStateWindow : ChatCondition
{
    public OpenStateWindow(string name, bool startedDetect, bool isUnlocked,  Action<List<ChatData>> onUnlocked,ChatData chatData) : base(name, startedDetect, isUnlocked, onUnlocked,chatData)
    {
    }
    public override bool Detect(string type, string value)
    {
        if(type=="AwakeWindow"&&value=="State")
        {
            return true;
        }
        return false;
    }
}