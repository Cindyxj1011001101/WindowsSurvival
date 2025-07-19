/// <summary>
/// 人力发电机
/// </summary>
public class HumanPoweredGenerator : Card
{
    public HumanPoweredGenerator()
    {
        events = new()
        {
            new Event("人力发电", "人力发电", Event_Generate, Judge_Generate),
        };
    }

    public void Event_Generate()
    {
        StateManager.Instance.OnEnvironmentChangeState(new ChangeEnvironmentStateArgs(GameManager.Instance.CurEnvironmentBag.PlaceData.placeType, EnvironmentStateEnum.Electricity, 10));
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Thirst, -5));
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Tired, 6));
        TimeManager.Instance.AddTime(60);
    }

    public bool Judge_Generate()
    {
        if (slot.Bag is EnvironmentBag environmentBag)
        {
            if (environmentBag.EnvironmentStateDict[EnvironmentStateEnum.HasCable].curValue == 1)
            {
                return true;
            }
        }
        return false;
    }
}