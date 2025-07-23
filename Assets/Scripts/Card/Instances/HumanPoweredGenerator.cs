/// <summary>
/// 人力发电机
/// </summary>
public class HumanPoweredGenerator : Card
{
    private HumanPoweredGenerator()
    {
        Events = new()
        {
            new Event("人力发电", "人力发电", Event_Generate, Judge_Generate),
        };
    }

    public void Event_Generate()
    {
        // 电力+10
        StateManager.Instance.ChangeElectricity(+10);
        // 水分-5
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Thirst, -5);
        // 清醒-6
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Sobriety, -6);
        // 消耗60分钟
        TimeManager.Instance.AddTime(60);
    }

    public bool Judge_Generate()
    {
        if (Slot == null || Slot.Bag == null || Slot.Bag is not EnvironmentBag) return false;

        var env = Slot.Bag as EnvironmentBag;
        return env.HasCable;
    }
}