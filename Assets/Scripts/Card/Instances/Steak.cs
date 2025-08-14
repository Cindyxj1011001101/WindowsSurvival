using System.Collections.Generic;

public class Steak : Card
{
    private Steak()
    {
        Events = new()
        {
            new Event("食用", "食用肉排", Event_Eat, null, () => 15,
            () => new Dictionary<PlayerStateEnum, float>()
            {
                { PlayerStateEnum.Fullness, 68 },
                { PlayerStateEnum.Health, 5 }
            })
        };
    }
    public void Event_Eat(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 68);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, 5);
        TimeManager.Instance.AddTime(15);
    }
}