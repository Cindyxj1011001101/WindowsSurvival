using System.Collections.Generic;

/// <summary>
/// 白爆矿
/// </summary>
public class WhiteBlastMine : Card
{
    private WhiteBlastMine()
    {
        Events = new()
        {
            new Event("敲碎", "会产生少量氧气",Event_Break, null, () => 3,
            () => new Dictionary<PlayerStateEnum, float>() { { PlayerStateEnum.Oxygen, 80 } })
        };
    }

    private void Event_Break(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Oxygen, +80);
        TimeManager.Instance.AddTime(3);
    }
}