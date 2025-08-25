using System;
using System.Collections.Generic;

/// <summary>
/// 小块肉
/// </summary>
public class LittleRawMeat : CookableCard
{
    private LittleRawMeat()
    {
        Events = new()
        {
            new Event("食用", "食用小块生肉", Event_Eat, null, () => 15,
            () => new Dictionary<PlayerStateEnum, float>() { { PlayerStateEnum.Fullness, 12 } ,{ PlayerStateEnum.San, -2 }, { PlayerStateEnum.Health, -3 }})
        };
    }

    private void OnRotton()
    {
        DestroyThis();
        AddCard("腐烂物", Bag);
    }

    private void Event_Eat(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        // 播放吃的音效
        if(SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("吃_01",true);
        //+12饱食
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 12);
        //-2精神值
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, -2);
        //-3健康
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, -3);
        //消耗15分钟
        TimeManager.Instance.AddTime(15);
    }

    protected override Action OnUpdate => () =>
    {
        TryGetComponent<FreshnessComponent>(out var component);
        component.Update(TimeManager.Instance.SettleInterval, OnRotton);
    };
}