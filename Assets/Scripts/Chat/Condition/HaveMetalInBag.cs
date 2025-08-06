using System;
using System.Collections.Generic;
using UnityEngine;

public class HaveMetalInBag : ChatCondition
{
    private bool triggered;
    public HaveMetalInBag(string name, bool startedDetect, bool isUnlocked,  Action<List<ChatData>> onUnlocked,ChatData chatData) : base(name, startedDetect, isUnlocked, onUnlocked,chatData)
    {
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