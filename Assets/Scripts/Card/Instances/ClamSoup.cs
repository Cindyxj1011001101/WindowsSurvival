public class ClamSoup : Card
{
    private ClamSoup()
    {
        Events = new()
        {
            new Event("食用", "食用蛤蜊浓汤", Event_Eat, null),
        };
    }
    public void Event_Eat(out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 20);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Thirst, 55);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, 10);
        TimeManager.Instance.AddTime(15);
    }
}