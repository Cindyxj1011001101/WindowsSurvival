using System;

public class OpenTechnologyWindow : Condition
{
    public OpenTechnologyWindow(string name, bool startedDetect, bool isUnlocked, Action onUnlocked) : base(name, startedDetect, isUnlocked, onUnlocked)
    { }

    public override bool Detect(string type, string value)
    {
        if(type=="AwakeWindow"&&value=="Study")
        {
            return true;
        }
        return false;
    }
}