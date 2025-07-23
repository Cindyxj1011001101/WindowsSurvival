using System;

public class CreateCrackFiller : Condition
{
    public CreateCrackFiller(string name, bool startedDetect, bool isUnlocked, Action onUnlocked) : base(name, startedDetect, isUnlocked, onUnlocked)
    {
        this.name = name;
        this.startedDetect = startedDetect;
        this.isUnlocked = isUnlocked;
        this.onUnlocked = onUnlocked;
    }

    public override bool Detect(string type, string value)
    {
        if(type=="Craft"&&value=="裂缝填充物")
        {
            return true;
        }
        return false;
    }
}