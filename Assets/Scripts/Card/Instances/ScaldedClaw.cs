using System.Collections.Generic;

public class ScaldedClaw : Card
{
    private ScaldedClaw()
    {
        Events=new()
        {
            new Event("食用", "食用白灼触手", Event_Eat, null, () => 45,
            () => new Dictionary<PlayerStateEnum, float>()
            {
                { PlayerStateEnum.Fullness, 66 },
                { PlayerStateEnum.San, -3 }
            })
        };
    }

    public void Event_Eat(out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 66);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, -3);
        TimeManager.Instance.AddTime(45);
    }
}