using System.Collections.Generic;

public class FriedInsectStick : Card
{
    private FriedInsectStick()
    {
        Events = new()
        {
            new Event("食用", "食用炸虫串", Event_Eat, null, () => 15,
            () => new Dictionary<PlayerStateEnum, float>()
            {
                { PlayerStateEnum.Fullness, 46 },
                { PlayerStateEnum.Thirst, -4 },
                { PlayerStateEnum.San, 8 }
            })
        };
    }

    private void Event_Eat(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("吃_01", true);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 46);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Thirst, -4);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, 8);
        TimeManager.Instance.AddTime(15);
    }
}