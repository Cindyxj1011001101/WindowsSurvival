using System;
using System.Collections.Generic;
using UnityEngine;

public class SobrietyLessThan30:ParagraphCondition
{
    public SobrietyLessThan30(string name, bool startedDetect, bool isUnlocked, Action<List<ParagraphData>> onUnlocked,ParagraphData paragraphData) : base(name, startedDetect, isUnlocked, onUnlocked) {
        ParagraphDatas.Add(paragraphData);
        Repeat = true;
    }

    public override bool Detect(string type, string value)
    {
        if (type == "PlayerSobriety" && value=="正常-疲劳")
        {
            return true;
        }
        return false;
    }
}