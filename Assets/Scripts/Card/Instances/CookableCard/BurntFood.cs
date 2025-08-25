using System.Collections.Generic;

/// <summary>
/// 烧焦的食物
/// </summary>
public class BurntFood : CookableCard
{
    private BurntFood()
    {
        Events = new()
        {
            new Event("食用", "食用烧焦的食物", Event_Eat, null, () => 15,
            () => new Dictionary<PlayerStateEnum, float>()
            {
                { PlayerStateEnum.Fullness, 10 },
                { PlayerStateEnum.Thirst, -20 },
                { PlayerStateEnum.Health, -5 },
                { PlayerStateEnum.BodyTemperature, 20 }
            })
        };
    }
    private void Event_Eat(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 10);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Thirst, -20);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, -5);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.BodyTemperature, 20);
        TimeManager.Instance.AddTime(15);
    }
}