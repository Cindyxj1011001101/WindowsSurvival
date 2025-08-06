using System;
using System.Collections.Generic;

public class FirstEnterCoralIsland:ParagraphCondition
{
    public static bool triggered=false;
    public FirstEnterCoralIsland(string name, bool startedDetect, bool isUnlocked,  Action<List<ParagraphData>> onUnlocked,ParagraphData paragraphData) : base(name, startedDetect, isUnlocked, onUnlocked) {
        AddData(paragraphData);
    }

    public override bool Detect(string type, string value)
    {
        if (triggered) return false;
        if (type == "EnterEnvironment" && value == "CoralCoast")
        {
            triggered = true;
            return true ;
        }
        return false;

    }
}