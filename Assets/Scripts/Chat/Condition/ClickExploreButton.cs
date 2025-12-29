using System;
using System.Collections.Generic;

/// <summary>
/// 对话触发条件：点击探索按钮时触发
/// 检测事件类型："Click"，值："Explore"
/// </summary>
public class ClickExploreButton : ChatCondition
{
    public ClickExploreButton(string name, bool startedDetect, bool isUnlocked,  Action<List<ChatData>> onUnlocked,ChatData chatData) : base(name, startedDetect, isUnlocked, onUnlocked,chatData)
    {
    }

    public override bool Detect(string type, string value)
    {
        if(type=="Click"&&value=="Explore")
        {
            return true;
        }
        return false;
    }
}