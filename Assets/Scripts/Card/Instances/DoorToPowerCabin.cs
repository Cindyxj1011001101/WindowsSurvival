/// <summary>
/// 通往动力舱的门
/// </summary>
public class DoorToPowerCabin : Card
{
    private DoorToPowerCabin()
    {
        events = new()
        {
            new Event("前往", "前往动力舱", Event_Move, null)
        };
    }

    public void Event_Move()
    {
        GameManager.Instance.Move(PlaceEnum.PowerCabin);
        TimeManager.Instance.AddTime(1);
    }
}