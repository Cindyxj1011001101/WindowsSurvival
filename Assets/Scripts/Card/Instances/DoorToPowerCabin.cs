using System.Collections.Generic;

/// <summary>
/// 通往动力舱的门
/// </summary>
public class DoorToPowerCabin : Card
{
    public DoorToPowerCabin()
    {
        //初始化参数
        cardName = "通往动力舱的门";
        cardDesc = "通往动力舱的门，可以通往动力舱。";
        //cardImage = Resources.Load<Sprite>("CardImage/通往动力舱的门");
        cardType = CardType.Place;
        maxStackNum = 1;
        moveable = true;
        weight = 0;
        curEndurance = maxEndurance = -1;
        tags = new();
        events = new List<Event>
        {
            new Event("前往", "前往动力舱", Event_Move, null)
        };
        components = new()
        {
            { typeof(PlaceComponent), new PlaceComponent(PlaceEnum.PowerCabin) },
        };
    }

    public void Event_Move()
    {
        GameManager.Instance.Move(PlaceEnum.PowerCabin);
        TimeManager.Instance.AddTime(1);
    }
}