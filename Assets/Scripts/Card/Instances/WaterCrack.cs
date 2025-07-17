using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
public class WaterCrack : Card
{
    public WaterCrack()
    {
        cardName = "渗水裂缝";
        cardDesc = "飞船外壳的裂缝，正不断地漏水进来，如果不赶紧堵起来的话，一切就都结束了。";
        cardType = CardType.Place;
        maxStackNum = 1;
        moveable = false;
        weight = 0f;
        events = new List<Event>();
        events.Add(new Event("堵住", "堵住渗水裂缝", Event_Fix, null));
        tags = new List<CardTag>();
        components = new();
    }

    public void Event_Fix()
    {
        Use();
        PlayerBag playerBag = GameManager.Instance.PlayerBag;
        foreach(CardSlot slot in playerBag.Slots)
        {
            if(slot.PeekCard().cardName=="补丁")
            {
                slot.PeekCard().Use();
                break;
            }
        }
        TimeManager.Instance.AddTime(15);
    }
    protected override System.Action OnUpdate => () =>
    {
        //每个渗水裂缝每回合会使飞船水平面高度+0.3，渗水裂缝所在的地点每回合-8氧气。
        EnvironmentBag environmentBag = GameManager.Instance.CurEnvironmentBag;
        if(environmentBag!=null)
        {
            StateManager.Instance.OnEnvironmentChangeState(new ChangeEnvironmentStateArgs(EnvironmentStateEnum.Height, 0.3f));
            (slot.Bag as EnvironmentBag).EnvironmentStateDict[EnvironmentStateEnum.Oxygen].curValue-=8;
            if((slot.Bag as EnvironmentBag).EnvironmentStateDict[EnvironmentStateEnum.Oxygen].curValue<=0)
            {
                (slot.Bag as EnvironmentBag).EnvironmentStateDict[EnvironmentStateEnum.Oxygen].curValue=0;
            }
        }
    };
}