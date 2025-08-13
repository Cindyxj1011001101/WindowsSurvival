using System;
using System.Collections.Generic;
using UnityEngine;

public class Day1Hour11FixUnConplished:ParagraphCondition
{
    public Day1Hour11FixUnConplished(string name, bool startedDetect, bool isUnlocked, Action<List<ParagraphData>> onUnlocked,ParagraphData paragraphData) : base(name, startedDetect, isUnlocked, onUnlocked) {
        ParagraphDatas.Add(paragraphData);
    }

    public override bool Detect(string type, string value)
    {
        if (type == "Day1Hour11" && value=="FixUnConplished")
        {
            return true;
        }
        return false;
    }
}