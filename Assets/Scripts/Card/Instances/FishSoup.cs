using System.Collections.Generic;

public class FishSoup : Card
{
    private FishSoup()
    {
        Events=new()
        {
            new Event("食用", "食用鱼汤", Event_Eat, null, () => 15,
            () => new Dictionary<PlayerStateEnum, float>()
            {
                { PlayerStateEnum.Fullness, 15 },
                { PlayerStateEnum.Thirst, 29 },
                { PlayerStateEnum.San, 12 },
                { PlayerStateEnum.Health, 12 },
                { PlayerStateEnum.PainLevel, -25 }
            })
        };
    }

    public void Event_Eat(out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 15);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Thirst, 29);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, 12);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, 12);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.PainLevel, -25);
        TimeManager.Instance.AddTime(15);
    }
}