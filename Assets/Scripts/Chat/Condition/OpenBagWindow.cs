using System;
using System.Collections.Generic;

/// <summary>
/// 对话触发条件：打开背包窗口时触发
/// 检测事件类型："AwakeWindow"，值："PlayerBag"
/// </summary>
public class OpenBagWindow : ChatCondition
{
    public OpenBagWindow(string name, bool startedDetect, bool isUnlocked,  Action<List<ChatData>> onUnlocked,ChatData chatData) : base(name, startedDetect, isUnlocked, onUnlocked,chatData)
    {
    }

    public override bool Detect(string type, string value)
    {
        if(type=="AwakeWindow"&&value=="PlayerBag")
        {
            return true;
        }
        return false;
    }
}