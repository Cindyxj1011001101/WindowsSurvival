using System;

public class OpenDetailBiscuit : Condition
{
    public OpenDetailBiscuit(string name, bool startedDetect, bool isUnlocked, Action onUnlocked) : base(name, startedDetect, isUnlocked, onUnlocked) { }

    public override bool Detect(string type, string value)
    {
        if(type=="Detail"&&value=="压缩饼干")
        {
            return true;
        }
        return false;
    }
}