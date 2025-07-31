using System.Collections.Generic;

public class SeaLizard : Card
{
    private SeaLizard()
    {
        Events = new()
        {
            new Event("食用", "食用海爬虫", Event_Eat, null, null, 15,
            new Dictionary<PlayerStateEnum, float>() { { PlayerStateEnum.Fullness, 6 }, { PlayerStateEnum.San, -3 }, { PlayerStateEnum.Itchiness, 25 } })
        };
    }

    public void Event_Eat()
    {
        DestroyThis();
        TimeManager.Instance.AddTime(15);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 6);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, -3);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Itchiness, 25);

    }
}
