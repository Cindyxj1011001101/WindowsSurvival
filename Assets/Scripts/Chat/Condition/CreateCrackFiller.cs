using System;
using System.Collections.Generic;

public class CreateCrackFiller : ChatCondition
{
    public CreateCrackFiller(string name, bool startedDetect, bool isUnlocked,  Action<List<ChatData>> onUnlocked,ChatData chatData) : base(name, startedDetect, isUnlocked, onUnlocked,chatData)
    {
    }

    public override bool Detect(string type, string value)
    {
        if(type=="Craft"&&value=="裂缝填充物")
        {
            return true;
        }
        return false;
    }
}