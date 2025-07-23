using System;

public class OpenStateWindow : Condition
{
    public OpenStateWindow(string name, bool startedDetect, bool isUnlocked, Action onUnlocked) : base(name, startedDetect, isUnlocked, onUnlocked) { }

    public override bool Detect(string type, string value)
    {
        if(type=="AwakeWindow"&&value=="State")
        {
            return true;
        }
        return false;
    }
}