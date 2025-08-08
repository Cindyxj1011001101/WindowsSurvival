public class ShellSashimi : Card
{
    private ShellSashimi()
    {
        Events=new()
        {
            new Event("食用", "食用贝类刺身", Event_Eat, null)
        };
    }

    public void Event_Eat(out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 34);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Thirst, 9);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, 10);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, -5);
        TimeManager.Instance.AddTime(15);
    }
}