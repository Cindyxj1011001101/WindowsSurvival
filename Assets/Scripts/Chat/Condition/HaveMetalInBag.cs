using System;
using UnityEngine;

public class HaveMetalInBag : Condition
{
    private bool triggered;
    public HaveMetalInBag(string name, bool startedDetect, bool isUnlocked, Action onUnlocked) : base(name, startedDetect, isUnlocked, onUnlocked)
    {
        this.name = name;
        this.startedDetect = startedDetect;
        this.isUnlocked = isUnlocked;
        this.onUnlocked = onUnlocked;
        triggered = false;
    }

    public override bool Detect(string type, string value)
    {
        return triggered;
    }
    public override bool OnCardChanges(Card card,int  add)
    {
        return card.CardName == "废金属";
    }
}