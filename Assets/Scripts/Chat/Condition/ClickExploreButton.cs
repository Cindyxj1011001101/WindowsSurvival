using System;
using System.Collections.Generic;

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