using System;
using System.Collections.Generic;

public class FirstOpenAirtightDoor:ParagraphCondition
{    
    public static bool triggered=false;
    public FirstOpenAirtightDoor(string name, bool startedDetect, bool isUnlocked,  Action<List<ParagraphData>> onUnlocked,ParagraphData paragraphData) : base(name, startedDetect, isUnlocked, onUnlocked) {
        AddData(paragraphData);
    }

    public override bool Detect(string type, string value)
    {
        if (triggered) return false;
        if (type == "Detail" && value == "气密舱门")
        {
            triggered = true;
            return true ;
        }
        return false;
    }
}