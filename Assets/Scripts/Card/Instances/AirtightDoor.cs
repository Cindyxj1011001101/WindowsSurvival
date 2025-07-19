/// <summary>
/// 气密舱门
/// </summary>
public class AirtightDoor : Card
{
    public AirtightDoor()
    {
        events = new()
        {
            new Event("进入飞船", "进入飞船", Event_Enter, Judge_Enter),
            new Event("离开飞船", "离开飞船", Event_Leave, Judge_Leave)
        };
    }

    public void Event_Enter()
    {
        GameManager.Instance.Move(PlaceEnum.Cockpit);
        TimeManager.Instance.AddTime(15);
    }

    public bool Judge_Enter()
    {
        return GameManager.Instance.CurEnvironmentBag.PlaceData.placeType == PlaceEnum.CoralCoast;
    }

    public void Event_Leave()
    {
        GameManager.Instance.Move(PlaceEnum.CoralCoast);
        TimeManager.Instance.AddTime(15);
    }

    public bool Judge_Leave()
    {
        return GameManager.Instance.CurEnvironmentBag.PlaceData.placeType == PlaceEnum.Cockpit;
    }
}