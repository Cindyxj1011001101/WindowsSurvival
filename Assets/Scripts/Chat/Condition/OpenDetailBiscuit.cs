using System;
using System.Collections.Generic;
using UnityEngine;

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