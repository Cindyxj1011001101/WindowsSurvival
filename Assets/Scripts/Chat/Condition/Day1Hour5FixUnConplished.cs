using System;
using System.Collections.Generic;
using UnityEngine;

public class Day1Hour5FixUnConplished:ParagraphCondition
{
    public Day1Hour5FixUnConplished(string name, bool startedDetect, bool isUnlocked, Action<List<ParagraphData>> onUnlocked,ParagraphData paragraphData) : base(name, startedDetect, isUnlocked, onUnlocked) {
        ParagraphDatas.Add(paragraphData);
    }

    public override bool Detect(string type, string value)
    {

        if (type == "Day1Hour5" && value=="FixUnConplished")
        {
            Debug.Log("通过检测");
            return true;
        }
        return false;
    }
}