using System;
using System.Collections.Generic;

/// <summary>
/// 磁性触手
/// </summary>
public class MagneticTentacle : Card
{
    private MagneticTentacle()
    {
        Events = new()
        {
            new Event("食用", "闻起来有铁锈味", Event_Eat, null, () => 30,
            () => new Dictionary<PlayerStateEnum, float>() { { PlayerStateEnum.Fullness, 14 }, { PlayerStateEnum.San, -6 }, { PlayerStateEnum.Health, -5 } })
        };
    }

    private void OnRotton()
    {
        DestroyThis();
        AddCard("废金属", Bag);
    }

    private void Event_Eat(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        // 播放吃的音效
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("吃_01", true);
        //+14饱食
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 14);
        //-6精神
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, -6);
        //-5健康
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, -5);
        //消耗30分钟
        TimeManager.Instance.AddTime(30);
    }

    protected override Action OnUpdate => () =>
    {
        TryGetComponent<FreshnessComponent>(out var component);
        component.Update(TimeManager.Instance.SettleInterval, OnRotton);
    };
}