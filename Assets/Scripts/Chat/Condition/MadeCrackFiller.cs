using System;
using System.Collections.Generic;

public class MadeCrackFiller:ParagraphCondition
{
    public MadeCrackFiller(string name, bool startedDetect, bool isUnlocked,  Action<List<ParagraphData>> onUnlocked,ParagraphData paragraphData) : base(name, startedDetect, isUnlocked, onUnlocked) {
        AddData(paragraphData);
    }

    public override bool Detect(string type, string value)
    {
        if (type == "Craft" && value == "裂缝填充物")
        {
            return true ;
        }
        return false;
    }
}