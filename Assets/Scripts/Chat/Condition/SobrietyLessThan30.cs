using System;
using UnityEngine;

public class SobrietyLessThan30:Condition
{
    public SobrietyLessThan30(string name, bool startedDetect, bool isUnlocked, Action onUnlocked) : base(name, startedDetect, isUnlocked, onUnlocked) {
        this.name = name;
        this.startedDetect = startedDetect;
        this.isUnlocked = isUnlocked;
        this.onUnlocked = onUnlocked;
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