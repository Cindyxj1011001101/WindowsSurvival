using System;

public class FinishResearchFix : Condition
{
    public FinishResearchFix(string name, bool startedDetect, bool isUnlocked, Action onUnlocked) : base(name, startedDetect, isUnlocked, onUnlocked) {
        this.name = name;
        this.startedDetect = startedDetect;
        this.isUnlocked = isUnlocked;
        this.onUnlocked = onUnlocked;
    }

    public override bool Detect(string type, string value)
    {
        if(type=="FinishResearch"&&value=="修理")
        {
            return true;
        }
        return false;
    }
}   