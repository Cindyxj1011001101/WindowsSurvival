using System.Collections.Generic;

public class CoalGrilledMeat : Card
{
    private CoalGrilledMeat()
    {
        Events = new()
        {
            new Event("食用", "黑金炭烤肉", Event_Eat, null, () => 15,
            () => new Dictionary<PlayerStateEnum, float>()
            {
                { PlayerStateEnum.Fullness, 94 },
                { PlayerStateEnum.Thirst, -10 },
                { PlayerStateEnum.San, 8 },
                { PlayerStateEnum.Health, 5 }
            })
        };
    }
    public void Event_Eat(out string tip)
    {
        StopUpdating();

        tip = string.Empty;
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 94);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Thirst, -10);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, 8);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, 5);
        TimeManager.Instance.AddTime(15);

        DestroyThis();
    }
}