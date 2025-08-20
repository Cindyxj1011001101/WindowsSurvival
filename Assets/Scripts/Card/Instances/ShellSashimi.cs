using System.Collections.Generic;

/// <summary>
/// 贝类刺身
/// </summary>
public class ShellSashimi : Card
{
    private ShellSashimi()
    {
        Events = new()
        {
            new Event("食用", "食用贝类刺身", Event_Eat, null, () => 15,
            () => new Dictionary<PlayerStateEnum, float>()
            {
                { PlayerStateEnum.Fullness, 44 },
                { PlayerStateEnum.Thirst, 14 },
                { PlayerStateEnum.San, 10 },
                { PlayerStateEnum.Health, -3 }
            })
        };
    }

    private void Event_Eat(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 44);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Thirst, 14);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, 10);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, -3);
        TimeManager.Instance.AddTime(15);
    }
}