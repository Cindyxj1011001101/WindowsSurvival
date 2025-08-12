using System;
using System.Collections.Generic;
using UnityEngine;

public class Day1Hour5FixConplished:ParagraphCondition
{
    public Day1Hour5FixConplished(string name, bool startedDetect, bool isUnlocked, Action<List<ParagraphData>> onUnlocked,ParagraphData paragraphData) : base(name, startedDetect, isUnlocked, onUnlocked) {
        ParagraphDatas.Add(paragraphData);
        Repeat = true;
    }

    public override bool Detect(string type, string value)
    {
        if (type == "Day1Hour5" && value=="FixUnConplished")
        {
            return true;
        }
        return false;
    }
}