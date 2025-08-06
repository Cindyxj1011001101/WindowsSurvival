using System;
using System.Collections.Generic;

public class HealthZero : ParagraphCondition
{
    public HealthZero(string name, bool startedDetect, bool isUnlocked,  Action<List<ParagraphData>> onUnlocked,ParagraphData paragraphData) : base(name, startedDetect, isUnlocked, onUnlocked)
    {
        AddData(paragraphData);
    }

    public override bool Detect(string type, string value)
    {
        if(type=="PlayerHealth"&&value=="0")
        {
            return true;
        }
        return false;
    }
}