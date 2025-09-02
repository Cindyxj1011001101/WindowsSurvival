using System.Collections.Generic;

/// <summary>
/// 熟水壶兰种子
/// </summary>
public class CookedKettleFlowerSeed : CookableCard
{
    private CookedKettleFlowerSeed()
    {
        Events = new()
        {
            new Event("食用", "", Event_Drink, null, () => 15,
            () => new Dictionary<PlayerStateEnum, float>()
            {
                { PlayerStateEnum.Fullness, 36 },
                { PlayerStateEnum.Thirst, 14 },
                { PlayerStateEnum.Health, 3 },
            })
        };
    }

    private void Event_Drink(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("吃_01", true);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 36);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Thirst, 14);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, 3);
        TimeManager.Instance.AddTime(15);
    }
}