/// <summary>
/// 白爆矿
/// </summary>
public class WhiteBlastMine : Card
{
    private WhiteBlastMine()
    {
        events = new()
        {
            new Event("敲碎", "敲碎白爆矿", Event_Break,null)
        };
    }

    public void Event_Break()
    {
        EnvironmentBag environmentBag = GameManager.Instance.CurEnvironmentBag;
        if (environmentBag.PlaceData.isIndoor)
        {
            StateManager.Instance.OnEnvironmentChangeState(new ChangeEnvironmentStateArgs(environmentBag.PlaceData.placeType, EnvironmentStateEnum.Oxygen, 80));
        }
        else
        {
            StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Oxygen, 80));
        }
        TimeManager.Instance.AddTime(3);
    }
}