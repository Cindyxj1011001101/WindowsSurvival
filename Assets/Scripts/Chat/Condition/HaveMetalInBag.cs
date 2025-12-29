using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对话触发条件：背包中有金属（废金属）时触发
/// 通过检测卡牌变化事件，当"废金属"卡牌被添加到背包时触发
/// </summary>
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