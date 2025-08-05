using System;

public class FirstOpenAirtightDoor:Condition
{    
    public static bool triggered=false;
    public FirstOpenAirtightDoor(string name, bool startedDetect, bool isUnlocked, Action onUnlocked) : base(name, startedDetect, isUnlocked, onUnlocked) {
        this.name = name;
        this.startedDetect = startedDetect;
        this.isUnlocked = isUnlocked;
        this.onUnlocked = onUnlocked;
    }

    public override bool Detect(string type, string value)
    {
        if (triggered) return false;
        if (type == "Detail" && value == "气密舱门")
        {
            triggered = true;
            return true ;
        }
        return false;
    }
}