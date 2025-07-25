/// <summary>
/// 白爆矿
/// </summary>
public class WhiteBlastMine : Card
{
    private WhiteBlastMine()
    {
        Events = new()
        {
            new Event("敲碎", "敲碎白爆矿", Event_Break, null)
        };
    }

    public void Event_Break()
    {
        DestroyThis();
        // 因为在室内环境加玩家氧气时会优先加到环境里，所以这里可以写直接加给玩家
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Oxygen, +80);
        TimeManager.Instance.AddTime(3);
    }
}