public class Steak : Card
{
    private Steak()
    {
        Events = new()
        {
            new Event("食用", "食用牛排", Event_Eat, null),
        };
    }
    public void Event_Eat()
    {
        DestroyThis();
        TimeManager.Instance.AddTime(15);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 55);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, 3);
    }
}