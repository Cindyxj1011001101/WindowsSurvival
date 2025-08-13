using System;
using System.Collections.Generic;

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