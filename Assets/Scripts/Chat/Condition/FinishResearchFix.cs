using System;
using System.Collections.Generic;
using UnityEngine;

public class FinishResearchFix : ParagraphCondition
{
    public FinishResearchFix(string name, bool startedDetect, bool isUnlocked,  Action<List<ParagraphData>> onUnlocked,ParagraphData paragraphData) : base(name, startedDetect, isUnlocked, onUnlocked) {
        AddData(paragraphData);
    }

    public override bool Detect(string type, string value)
    {
        if(type=="FinishResearch"&&value=="修理")
        {

            return true;
        }
        return false;
    }
}   