using System;
using System.Collections.Generic;
using UnityEngine;

public class WaterLevel100:ParagraphCondition
{
    public WaterLevel100(string name, bool startedDetect, bool isUnlocked, Action<List<ParagraphData>> onUnlocked,ParagraphData paragraphData) : base(name, startedDetect, isUnlocked, onUnlocked) {
        ParagraphDatas.Add(paragraphData);
    }

    public override bool Detect(string type, string value)
    {
        if (type == "WaterLevel" && value=="100")
        {
            return true;
        }
        return false;
    }
}