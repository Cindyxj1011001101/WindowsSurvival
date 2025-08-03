public class KitchenFoes : Card
{
    private KitchenFoes()
    {
        Events=new()
        {
            new Event("食用", "食用厨房恶物", Event_Eat, null)
        };
    }

    public void Event_Eat(out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        TimeManager.Instance.AddTime(15);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 10);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, -6);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, -4);
    }
}