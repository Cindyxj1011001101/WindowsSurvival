/// <summary>
/// 通往维生舱的门
/// </summary>
public class DoorToLifeSupportCabin : Card
{
    public DoorToLifeSupportCabin()
    {
        //初始化参数
        cardName = "通往维生舱的门";
        cardDesc = "通往维生舱的门，可以通往维生舱。";
        cardType = CardType.Place;
        maxStackNum = 1;
        moveable = false;
        weight = 0;
        events = new()
        {
            new Event("前往", "前往维生舱", Event_Move, null)
        };
        components = new()
        {
            { typeof(PlaceComponent), new PlaceComponent(PlaceEnum.LifeSupportCabin) },
        };
    }

    public void Event_Move()
    {
        GameManager.Instance.Move(PlaceEnum.LifeSupportCabin);
        TimeManager.Instance.AddTime(1);
    }
}