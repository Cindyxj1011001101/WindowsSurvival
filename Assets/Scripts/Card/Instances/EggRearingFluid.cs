using System.Collections.Generic;

/// <summary>
/// 育卵液
/// </summary>
public class EggRearingFluid : Card
{
    private EggRearingFluid()
    {
        Events = new()
        {
            new Event("饮用", "", Event_Drink, null, () => 3,
            () => new Dictionary<PlayerStateEnum, float>()
            {
                { PlayerStateEnum.Thirst, 40 },
                { PlayerStateEnum.Fullness, 10 },
            })
        };
    }

    private void Event_Drink(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        // 播放喝水的音效
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("喝_01", true);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Thirst, 40);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 10);
        TimeManager.Instance.AddTime(3);
    }
}