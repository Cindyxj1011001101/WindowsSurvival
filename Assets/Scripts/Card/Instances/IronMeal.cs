using System.Collections.Generic;

public class IronMeal : Card
{
    private IronMeal()
    {
        Events = new()
        {
            new Event("食用", "食用铁齿铜牙餐", Event_Eat, null, () => 30,
            () => new Dictionary<PlayerStateEnum, float>()
            {
                { PlayerStateEnum.Fullness, 35 },
                { PlayerStateEnum.San, -6 },
                { PlayerStateEnum.Health, -7 },
                { PlayerStateEnum.Itchiness, 50 }
            })
        };
    }
    private void Event_Eat(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 35);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, -6);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, -7);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Itchiness, 50);
        TimeManager.Instance.AddTime(30);
    }
}   