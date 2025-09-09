using System.Collections.Generic;

public class KitchenFoes : Card
{
    private KitchenFoes()
    {
        Events=new()
        {
            new Event("食用", "食用厨房恶物", Event_Eat, null, () => 15,
            () => new Dictionary<PlayerStateEnum, float>()
            {
                { PlayerStateEnum.Fullness, 13 },
                { PlayerStateEnum.San, -6 },
                { PlayerStateEnum.Health, -4 }
            })
        };
    }

    private void Event_Eat(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("吃_01", true);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 13);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, -6);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, -4);
        TimeManager.Instance.AddTime(15);
    }
}