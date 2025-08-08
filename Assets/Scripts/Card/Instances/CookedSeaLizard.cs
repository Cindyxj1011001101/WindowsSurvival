using System.Collections.Generic;

public class CookedSeaLizard : Card
{
    private CookedSeaLizard()
    {
        Events = new()
        {
            new Event("食用", "食用熟海爬虫", Event_Eat, null,() => 15,
            () => new Dictionary<PlayerStateEnum, float>() { { PlayerStateEnum.Fullness, 9 },{ PlayerStateEnum.Itchiness, 8 } })
        };
    }

    public void Event_Eat(out string tip)
    {
        tip = string.Empty;
        DestroyThis();

        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 9);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Itchiness, 8);
        TimeManager.Instance.AddTime(15);
    }
}
