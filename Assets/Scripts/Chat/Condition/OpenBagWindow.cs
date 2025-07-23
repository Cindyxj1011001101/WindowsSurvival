using System;

public class OpenBagWindow : Condition
{
    public OpenBagWindow(string name, bool startedDetect, bool isUnlocked, Action onUnlocked) : base(name, startedDetect, isUnlocked, onUnlocked) { }

    public override bool Detect(string type, string value)
    {
        if(type=="AwakeWindow"&&value=="PlayerBag")
        {
            return true;
        }
        return false;
    }
}