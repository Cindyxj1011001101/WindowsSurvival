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
        EventManager.Instance.AddListener<ChangePlayerBagCardsArgs>(EventType.ChangePlayerBagCards,OnCardChanges);
    }

    public override bool Detect(string type, string value)
    {
        if(triggered)EventManager.Instance.RemoveListener<ChangePlayerBagCardsArgs>(EventType.ChangePlayerBagCards,OnCardChanges);
        return triggered;
    }
    public void OnCardChanges(ChangePlayerBagCardsArgs args)
    {
        if (args.card.CardName == "废金属"&& args.add > 0)
        {
            triggered = true;
        }
    }
}