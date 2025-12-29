using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对话触发条件：打开压缩饼干详情时触发
/// 检测事件类型："Detail"，值："压缩饼干"
/// </summary>
public class OpenDetailBiscuit : ChatCondition
{
    public OpenDetailBiscuit(string name, bool startedDetect, bool isUnlocked,  Action<List<ChatData>> onUnlocked,ChatData chatData) : base(name, startedDetect, isUnlocked, onUnlocked,chatData)
    {
    }

    public override bool Detect(string type, string value)
    {
        if(type=="Detail"&&value=="压缩饼干")
        {
            return true;
        }
        return false;
    }
}