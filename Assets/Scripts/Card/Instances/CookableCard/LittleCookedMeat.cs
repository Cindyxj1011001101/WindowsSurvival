using System.Collections.Generic;

public class LittleCookedMeat : CookableCard
{
    private LittleCookedMeat()
    {
        Events = new()
        {
            new Event("食用", "食用小块熟肉", Event_Eat, null, () => 15,
            () => new Dictionary<PlayerStateEnum, float>()
            {
                { PlayerStateEnum.Fullness, 18 },
                { PlayerStateEnum.Health, 1 },
            })
        };
    }
    private void Event_Eat(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("吃_01", true);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 18);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, 1);
        TimeManager.Instance.AddTime(15);
    }
}