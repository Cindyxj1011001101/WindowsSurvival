using System.Collections.Generic;

public class CoalGrilledMeat : Card
{
    private CoalGrilledMeat()
    {
        Events = new()
        {
            new Event("食用", "有着一层酥脆的皮", Event_Eat, null, () => 15,
            () => new Dictionary<PlayerStateEnum, float>()
            {
                { PlayerStateEnum.Fullness, 105 },
                { PlayerStateEnum.Thirst, -10 },
                { PlayerStateEnum.San, 8 },
                { PlayerStateEnum.Health, 5 }
            })
        };
    }
    private void Event_Eat(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("吃_01", true);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 105);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Thirst, -10);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, 8);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, 5);
        TimeManager.Instance.AddTime(15);
        
}
}