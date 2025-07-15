using System.Collections.Generic;
using UnityEngine;

public enum EventTypeEnum
{
    CanTriggerEvent,// 判断某一卡牌行为是否可执行
    TriggerEvent,//执行某一卡牌行为
    //获取某一事件参数
    //获取卡牌参数
    Fresh,//卡牌腐烂
    Endurance,//卡牌损坏
}
//水瓶鱼
public class AquariusFish:Card
{
    public int maxFresh;//最大新鲜度
    public int curFresh;//当前新鲜度
    public int maxEndurance;//最大耐久度
    public int curEndurance;//当前耐久度

    //构造函数(可增加多参构造实现非满新鲜与耐久的卡牌初始化)
    public AquariusFish()
    {
        //初始化参数
        cardName = "水瓶鱼";
        cardDesc = "水瓶鱼";
        cardImage = Resources.Load<Sprite>("CardImage/水瓶鱼");
        cardType = CardType.Creature;
        maxFresh = 100;
        curFresh = 100;
        maxEndurance = 100;
        curEndurance = 100;
        maxStackNum = 1;
        moveable = true;
        weight = 1;
        events = new List<Event>();
    }

    public override void Use()
    {
        return;
    }

    public override void Fresh()
    {
        return;
    }
}