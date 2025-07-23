/// <summary>
/// 通往维生舱的门
/// </summary>
public class DoorToLifeSupportCabin : Card
{
    private DoorToLifeSupportCabin()
    {
        Events = new()
        {
            new Event("前往", "前往维生舱", Event_Move, null)
        };
    }

    public void Event_Move()
    {
        SoundManager.Instance.PlaySound("飞船门_02", true);
        GameManager.Instance.Move(PlaceEnum.LifeSupportCabin);
        TimeManager.Instance.AddTime(1);
    }
}