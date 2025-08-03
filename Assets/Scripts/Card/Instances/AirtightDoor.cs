/// <summary>
/// 气密舱门
/// </summary>
public class AirtightDoor : Card
{
    private AirtightDoor()
    {
        Events = new()
        {
            new Event("进入飞船", "进入飞船", Event_Enter, Judge_Enter, null, 15),
            new Event("离开飞船", "离开飞船", Event_Leave, Judge_Leave, null, 15)
        };
    }

    public void Event_Enter(out string tip)
    {
        tip = string.Empty;
        SoundManager.Instance.PlaySound("飞船门_02", true);
        GameManager.Instance.Move(PlaceEnum.Cockpit);
        TimeManager.Instance.AddTime(15);
    }
    public bool Judge_Enter()
    {

        return GameManager.Instance.CurEnvironmentBag.PlaceData.placeType == PlaceEnum.CoralCoast;
    }

    public void Event_Leave(out string tip)
    {
        tip = string.Empty;
        SoundManager.Instance.PlaySound("飞船门_02", true);
        GameManager.Instance.Move(PlaceEnum.CoralCoast);
        TimeManager.Instance.AddTime(15);
    }

    public bool Judge_Leave()
    {
        return GameManager.Instance.CurEnvironmentBag.PlaceData.placeType == PlaceEnum.Cockpit;
    }
}