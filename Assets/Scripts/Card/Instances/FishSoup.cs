public class FishSoup : Card
{
    private FishSoup()
    {
        Events=new()
        {
            new Event("食用", "食用鱼汤", Event_Eat, null)
        };
    }

    public void Event_Eat(out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 10);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Thirst, 18);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, 12);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, 12);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.PainLevel, -25);
        TimeManager.Instance.AddTime(15);
    }
}