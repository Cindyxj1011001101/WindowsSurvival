using System.Collections.Generic;

/// <summary>
/// 人力发电机
/// </summary>
public class HumanPoweredGenerator : Card
{
    private HumanPoweredGenerator()
    {
        Events = new()
        {
            new Event("人力发电", "踩轮子发电", Event_Generate, Judge_Generate, () => 60,
            () => new Dictionary<PlayerStateEnum, float>() { { PlayerStateEnum.Thirst, -5 }, { PlayerStateEnum.Sobriety, -6 } },
            () => new Dictionary < EnvironmentStateEnum, float >() { { EnvironmentStateEnum.Electricity, 10 } })
        };
    }

    public void Event_Generate(out string tip)
    {
        tip = string.Empty;
        // 电力+10
        StateManager.Instance.ChangeElectricity(+10);
        // 水分-5
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Thirst, -5);
        // 清醒-6
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Sobriety, -6);
        // 消耗60分钟
        TimeManager.Instance.AddTime(60);
    }

    public bool Judge_Generate(out string hint)
    {
        hint = string.Empty;

        var env = Slot.Bag as EnvironmentBag;
        if (!env.HasCable)
        {
            hint = "需要将该地区连入电网";
            return false;
        }
        return true;
    }
}