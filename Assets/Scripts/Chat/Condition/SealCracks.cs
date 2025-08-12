using System;
using System.Collections.Generic;
using UnityEngine;

public class SealCracks:ParagraphCondition
{
    public SealCracks(string name, bool startedDetect, bool isUnlocked, Action<List<ParagraphData>> onUnlocked,ParagraphData paragraphData) : base(name, startedDetect, isUnlocked, onUnlocked) {
        ParagraphDatas.Add(paragraphData);
        Repeat = true;
    }

    public override bool Detect(string type, string value)
    {
        if (type == "渗水裂缝" && value=="堵住")
        {
            return true;
        }
        return false;
    }
}