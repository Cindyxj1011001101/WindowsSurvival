using System.Collections.Generic;

/// <summary>
/// 水壶兰种子
/// </summary>
public class KettleFlowerSeed : CookableCard
{
    private KettleFlowerSeed()
    {
        Events = new()
        {
            new Event("食用", "", Event_Drink, null, () => 15,
            () => new Dictionary<PlayerStateEnum, float>()
            {
                { PlayerStateEnum.Fullness, 25 },
                { PlayerStateEnum.Thirst, 14 },
            })
        };
    }

    private void Event_Drink(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 25);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Thirst, 14);
        TimeManager.Instance.AddTime(15);
    }
}