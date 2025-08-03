using UnityEngine;

public class FriedInsectStick : Card
{
    private FriedInsectStick()
    {
        Events=new()
        {
            new Event("食用", "食用炸虫串", Event_Eat, null)
        };
    }

    public void Event_Eat(out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        TimeManager.Instance.AddTime(15);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 36);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Thirst, -4);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, -8);

    }
}