public class CookedSeaLizard : Card
{
    private CookedSeaLizard()
    {
        Events = new()
        {
            new Event("食用", "食用熟海爬虫", Event_Eat, null)
        };
    }

    public void Event_Eat(out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        TimeManager.Instance.AddTime(15);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 9);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Itchiness, 8);
        
    }
}
