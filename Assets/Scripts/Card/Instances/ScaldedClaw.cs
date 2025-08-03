public class ScaldedClaw : Card
{
    private ScaldedClaw()
    {
        Events=new()
        {
            new Event("食用", "食用白灼触手", Event_Eat, null)
        };
    }

    public void Event_Eat(out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        TimeManager.Instance.AddTime(45);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 56);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, -3);
    }
}