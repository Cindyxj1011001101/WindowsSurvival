using System.Collections.Generic;

/// <summary>
/// 盐水
/// </summary>
public class SalineWater : Card
{
    private SalineWater()
    {
        Events = new()
        {
            new Event("饮用", "会导致脱水", Event_Drink, null, () => 3,
            () => new Dictionary<PlayerStateEnum, float>()
            {
                { PlayerStateEnum.Thirst, -25 },
            })
        };
    }

    private void Event_Drink(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Thirst, -25);
        TimeManager.Instance.AddTime(3);
    }
}