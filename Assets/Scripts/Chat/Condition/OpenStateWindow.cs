using System;
using System.Collections.Generic;

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