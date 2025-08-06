using System;
using System.Collections.Generic;

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