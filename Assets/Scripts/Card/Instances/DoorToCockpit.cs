/// <summary>
/// 通往驾驶室的门
/// </summary>
public class DoorToCockpit : Card
{
    public DoorToCockpit()
    {
        //初始化参数
        cardName = "通往驾驶室的门";
        cardDesc = "通往驾驶室的门，可以通往驾驶室。";
        cardType = CardType.Place;
        maxStackNum = 1;
        moveable = false;
        weight = 0;
        events = new()
        {
            new Event("前往", "前往驾驶室", Event_Move, null)
        };
        components = new()
        {
            { typeof(PlaceComponent), new PlaceComponent(PlaceEnum.Cockpit) },
        };
    }

    public void Event_Move()
    {
        GameManager.Instance.Move(PlaceEnum.Cockpit);
        TimeManager.Instance.AddTime(1);
    }
}