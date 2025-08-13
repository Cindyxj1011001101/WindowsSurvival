using System;
using System.Collections.Generic;
using UnityEngine;

public class WaterLevel70:ParagraphCondition
{
    public float LastWaterLevel=0;
    public WaterLevel70(string name, bool startedDetect, bool isUnlocked, Action<List<ParagraphData>> onUnlocked,ParagraphData paragraphData) : base(name, startedDetect, isUnlocked, onUnlocked) {
        ParagraphDatas.Add(paragraphData);
        Repeat = true;
    }

    public override bool Detect(string type, string value)
    {
        if (type == "WaterLevel"&& float.TryParse(value, out float waterLevel))
        {
            if (LastWaterLevel < 70 && waterLevel >= 70)
            {
                LastWaterLevel=waterLevel;
                return true;
            }
            LastWaterLevel=waterLevel;
        }
        return false;
    }
}