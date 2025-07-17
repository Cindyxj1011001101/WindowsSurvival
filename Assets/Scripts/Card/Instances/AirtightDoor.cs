using System.Collections.Generic;
public class AirtightDoor : Card
{
    public AirtightDoor()
    {
        cardName = "气密舱门";
        cardDesc = "气密舱门实际上是两层门，员工手册上说这两道门永远不能同时打开。两道门中间有一个很小的过渡仓，通过分段加压/减压，让员工可以在不影响舱内环境的情况下安全进出。";
        cardType = CardType.Place;
        maxStackNum = 1;
        moveable = false;
        weight = 0f;
        events = new List<Event>();
        events.Add(new Event("进入飞船", "进入飞船", Event_Enter, Judge_Enter));
        events.Add(new Event("离开飞船", "离开飞船", Event_Leave, Judge_Leave));
        tags = new List<CardTag>();
    }

    public void Event_Enter()
    {
        GameManager.Instance.Move(PlaceEnum.Cockpit);
        TimeManager.Instance.AddTime(15);
    }

    public bool Judge_Enter()
    {
        return GameManager.Instance.CurEnvironmentBag.PlaceData.placeType==PlaceEnum.CoralCoast;
    }

    public void Event_Leave()
    {
        GameManager.Instance.Move(PlaceEnum.CoralCoast);
        TimeManager.Instance.AddTime(15);
    }

    public bool Judge_Leave()
    {
         return GameManager.Instance.CurEnvironmentBag.PlaceData.placeType==PlaceEnum.Cockpit;
    }
}