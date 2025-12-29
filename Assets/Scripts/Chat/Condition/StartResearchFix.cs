using System;
using System.Collections.Generic;

/// <summary>
/// 对话触发条件：开始研究修复时触发
/// 检测事件类型："StartResearch"，值："修理"
/// </summary>
public class StartResearchFix : ChatCondition
{
    public StartResearchFix(string name, bool startedDetect, bool isUnlocked,  Action<List<ChatData>> onUnlocked,ChatData chatData) : base(name, startedDetect, isUnlocked, onUnlocked,chatData)
    {
        
    }

    public override bool Detect(string type, string value)
    {
        if(type=="StartResearch"&&value=="修理")
        {
            return true;
        }
        return false;
    }
}