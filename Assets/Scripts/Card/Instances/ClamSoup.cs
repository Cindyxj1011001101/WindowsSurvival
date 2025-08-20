using System.Collections.Generic;

public class ClamSoup : Card
{
    private ClamSoup()
    {
        Events = new()
        {
            new Event("食用", "食用蛤蜊浓汤", Event_Eat, null, () => 15,
            () => new Dictionary<PlayerStateEnum, float>()
            {
                { PlayerStateEnum.Fullness, 30 },
                { PlayerStateEnum.Thirst, 60 },
                { PlayerStateEnum.San, 10 }
            })
        };
    }
    private void Event_Eat(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 30);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Thirst, 60);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, 10);
        TimeManager.Instance.AddTime(15);
    }
}