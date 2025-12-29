using System;
using System.Collections.Generic;

/// <summary>
/// 对话触发条件：打开位置窗口（环境背包）时触发
/// 检测事件类型："AwakeWindow"，值："EnvironmentBag"
/// </summary>
public class OpenLocationWindow : ChatCondition
{
    public OpenLocationWindow(string name, bool startedDetect, bool isUnlocked,  Action<List<ChatData>> onUnlocked,ChatData chatData) : base(name, startedDetect, isUnlocked, onUnlocked,chatData)
    {
    }

    public override bool Detect(string type, string value)
    {
        if(type=="AwakeWindow"&&value=="EnvironmentBag")
        {
            return true;
        }
        return false;
    }
}