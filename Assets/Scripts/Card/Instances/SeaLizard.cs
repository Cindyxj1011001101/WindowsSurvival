public class SeaLizard : Card
{
    private SeaLizard()
    {
        Events = new()
        {
            new Event("食用", "食用海爬虫", Event_Eat, null)
        };
    }

    public void Event_Eat()
    {
        DestroyThis();
        TimeManager.Instance.AddTime(15);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 6);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, -3);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Itchiness, 25);

    }
}
