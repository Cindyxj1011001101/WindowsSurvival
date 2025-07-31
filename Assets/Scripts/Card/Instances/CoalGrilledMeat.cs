public class CoalGrilledMeat : Card
{
    private CoalGrilledMeat()
    {
        Events = new()
        {
            new Event("食用", "黑金炭烤肉", Event_Eat, null),
        };
    }
    public void Event_Eat()
    {
        DestroyThis();
        TimeManager.Instance.AddTime(15);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 78);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Thirst, -10);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, 8);
    }
}