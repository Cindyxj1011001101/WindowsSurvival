using System;

public class HaveMetalInBag : Condition
{
    public HaveMetalInBag(string name, bool startedDetect, bool isUnlocked, Action onUnlocked) : base(name, startedDetect, isUnlocked, onUnlocked)
    {
        this.name = name;
        this.startedDetect = startedDetect;
        this.isUnlocked = isUnlocked;
        this.onUnlocked = onUnlocked;
    }

    public override bool Detect(string type, string value)
    {
        if(type=="Bag"&&value=="废金属")
        {
            return true;
        }
        return false;
    }
}