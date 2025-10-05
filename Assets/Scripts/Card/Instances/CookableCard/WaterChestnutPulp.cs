using System.Collections.Generic;

/// <summary>
/// 四角菱果肉
/// </summary>
public class WaterChestnutPulp : CookableCard
{
    private WaterChestnutPulp()
    {
        Events = new()
        {
            new Event("食用", "", Event_Eat, null, () => 15,
            () => new Dictionary < PlayerStateEnum, float >() { { PlayerStateEnum.Fullness, +10 } })
        };
    }

    private void Event_Eat(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("吃_01", true);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, +10);
        TimeManager.Instance.AddTime(15);
    }
}
