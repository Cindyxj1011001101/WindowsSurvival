using System;

public class ClickExploreButton : Condition
{
    public ClickExploreButton(string name, bool startedDetect, bool isUnlocked, Action onUnlocked) : base(name, startedDetect, isUnlocked, onUnlocked)
    {
        this.name = name;
        this.startedDetect = startedDetect;
        this.isUnlocked = isUnlocked;
        this.onUnlocked = onUnlocked;
    }

    public override bool Detect(string type, string value)
    {
        if(type=="Click"&&value=="Explore")
        {
            return true;
        }
        return false;
    }
}