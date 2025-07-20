/// <summary>
/// 通往驾驶室的门
/// </summary>
public class DoorToCockpit : Card
{
    private DoorToCockpit()
    {
        events = new()
        {
            new Event("前往", "前往驾驶室", Event_Move, null)
        };
    }

    public void Event_Move()
    {
        SoundManager.Instance.PlaySound("飞船门_02", true);
        GameManager.Instance.Move(PlaceEnum.Cockpit);
        TimeManager.Instance.AddTime(1);
    }
}