using System.Collections.Generic;

public class CookedOysterMeat : CookableCard
{
    private CookedOysterMeat()
    {
        Events = new()
        {
            new Event("食用", "食用熟贝肉", Event_Eat, null, () => 3,
            () => new Dictionary<PlayerStateEnum, float>()
            {
                { PlayerStateEnum.Fullness, 8 },
                { PlayerStateEnum.Health, 1 },
                { PlayerStateEnum.San, 1 },
            })
        };
    }
    private void Event_Eat(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 8);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, 1);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, 1);
        TimeManager.Instance.AddTime(3);
    }
}