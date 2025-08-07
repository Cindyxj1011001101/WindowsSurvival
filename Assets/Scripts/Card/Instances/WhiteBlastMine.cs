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

    public void Event_Break(out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        // 因为在室内环境加玩家氧气时会优先加到环境里，所以这里可以写直接加给玩家
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Oxygen, +80);
        TimeManager.Instance.AddTime(3);
    }
}