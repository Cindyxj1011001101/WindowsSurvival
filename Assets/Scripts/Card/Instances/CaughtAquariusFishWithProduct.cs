using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 被捉住的水瓶鱼
/// </summary>
public class CaughtAquariusFishWithProduct : Card
{
    public CaughtAquariusFishWithProduct()
    {
        //初始化参数
        cardName = "被捉住的水瓶鱼";
        cardDesc = "一只水瓶鱼，其怀孕时体内的育卵液是重要的淡水来源。";
        CardImage = Resources.Load<Sprite>("CardImage/被捉住的水瓶鱼有产物");
        cardType = CardType.Creature;
        maxStackNum = 5;
        moveable = false;
        weight = 1.1f;
        curEndurance = maxEndurance = 1;
        tags = new List<CardTag>();
        events = new List<Event>
        {
            new Event("饮用", "饮用水瓶鱼", Event_Drink, null),
            new Event("放生", "放生水瓶鱼", Event_Release, Judge_Release),
        };
        components = new()
        {
            { typeof(FreshnessComponent), new FreshnessComponent(1440, OnFreshnessChanged) },
            { typeof(ProgressComponent), new ProgressComponent(5760, null) },
        };
    }

    public CaughtAquariusFishWithProduct(int progress) : this()
    {
        TryGetComponent<ProgressComponent>(out var component);
        component.progress = progress;
    }

    public void Event_Drink()
    {
        Use();
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Thirst, 15));
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Fullness, 4));
        TimeManager.Instance.AddTime(15);
    }

    public void Event_Release()
    {
        Use();
        // 地点中增加一个水瓶鱼
        TryGetComponent<ProgressComponent>(out var component);
        GameManager.Instance.AddCard(new AquariusFishWithProduct(component.progress), false);
    }

    public bool Judge_Release()
    {
        return GameManager.Instance.CurEnvironmentBag.PlaceData.isInWater;
    }

    private void OnFreshnessChanged(int freshness)
    {
        if (freshness == 0)
        {
            // 腐烂
            Use();
            // 其他行为
        }
    }

    protected override Action OnUpdate => () =>
    {
        TryGetComponent<FreshnessComponent>(out var component);
        component.Update(TimeManager.Instance.SettleInterval);
        // 因为产物进度不会变化，所以这里没必要添加它的Update
    };
}