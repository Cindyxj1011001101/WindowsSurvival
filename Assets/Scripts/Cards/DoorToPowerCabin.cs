using System.Collections.Generic;
using UnityEngine;

//通往动力舱的门
public class DoorToPowerCabin:Card
{


    //构造函数(可增加多参构造实现非满新鲜与耐久的卡牌初始化)
    public DoorToPowerCabin()
    {
        //初始化参数
        cardName = "通往动力舱的门";
        cardDesc = "通往动力舱的门，可以通往动力舱。";
        cardImage = Resources.Load<Sprite>("CardImage/通往动力舱的门");
        cardType = CardType.Place;
        maxStackNum =1;
        moveable = true;
        weight = 0.3f;
        events = new List<Event>();
        events.Add(new Event("前往", "前往动力舱", Event_Move, () => Judge_Move()));
    }

    public void Event_Move()
    {
        TimeManager.Instance.AddTime(1);
        GameManager.Instance.Move(PlaceEnum.PowerCabin);

    }

    public bool Judge_Move()
    {
        return true;
    }

}