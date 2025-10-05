using System.Collections.Generic;

/// <summary>
/// 烤四角菱果肉
/// </summary>
public class CookedWaterChestnutPulp : CookableCard
{
    private CookedWaterChestnutPulp()
    {
        Events = new()
        {
            new Event("食用", "", Event_Eat, null, () => 15,
            () => new Dictionary < PlayerStateEnum, float >() { { PlayerStateEnum.Fullness, +16 } })
        };
    }

    private void Event_Eat(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("吃_01", true);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, +16);
        TimeManager.Instance.AddTime(15);
    }
}
