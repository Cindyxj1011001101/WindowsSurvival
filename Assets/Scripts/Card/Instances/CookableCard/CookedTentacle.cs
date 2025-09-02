using System.Collections.Generic;

/// <summary>
/// 熟触手
/// </summary>
public class CookedTentacle : CookableCard
{
    private CookedTentacle()
    {
        Events = new()
        {
            new Event("食用", "食用熟触手", Event_Eat, null, () => 30,
            () => new Dictionary<PlayerStateEnum, float>()
            {
                { PlayerStateEnum.Fullness, 24 },
                { PlayerStateEnum.San, -1 },
            })
        };
    }
    private void Event_Eat(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("吃_01", true);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 24);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, -1);
        TimeManager.Instance.AddTime(30);
    }
}