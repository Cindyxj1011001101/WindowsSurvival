using System;

public class OpenLocationWindow : Condition
{
    public OpenLocationWindow(string name, bool startedDetect, bool isUnlocked, Action onUnlocked) : base(name, startedDetect, isUnlocked, onUnlocked)
    {
        this.name = name;
        this.startedDetect = startedDetect;
        this.isUnlocked = isUnlocked;
        this.onUnlocked = onUnlocked;
    }

    public override bool Detect(string type, string value)
    {
        if(type=="AwakeWindow"&&value=="EnvironmentBag")
        {
            return true;
        }
        return false;
    }
}