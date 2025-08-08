public class IronMeal : Card
{
    private IronMeal()
    {
        Events = new()
        {
            new Event("食用", "食用铁齿铜牙餐", Event_Eat, null),
        };
    }
    public void Event_Eat(out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 29);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, -6);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, -7);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Itchiness, 50);
        TimeManager.Instance.AddTime(30);
    }
}   