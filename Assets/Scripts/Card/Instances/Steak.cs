public class Steak : Card
{
    private Steak()
    {
        Events = new()
        {
            new Event("食用", "食用肉排", Event_Eat, null),
        };
    }
    public void Event_Eat(out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 55);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, 3);
        TimeManager.Instance.AddTime(15);
    }
}