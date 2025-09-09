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
                { PlayerStateEnum.Fullness, 36 },
                { PlayerStateEnum.Thirst, 66 },
                { PlayerStateEnum.San, 15 }
            })
        };
    }
    private void Event_Eat(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("喝_01", true);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 36);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Thirst, 66);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, 15);
        TimeManager.Instance.AddTime(15);
        
    }
}