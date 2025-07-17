using System.Collections.Generic;
using UnityEngine;

//被捉住的水瓶鱼
public class CaughtAquariusFish:Card
{
    public int maxProductProcess;//最大生产进度
    public int curProductProcess;//当前生产进度
    public int maxProductNum;//最大生产数量
    public int curProductNum;//当前生产数量
    public int maxFresh;//最大新鲜度
    public int curFresh;//当前新鲜度

    //构造函数(可增加多参构造实现非满新鲜与耐久的卡牌初始化)
    public CaughtAquariusFish()
    {
        //初始化参数
        cardName = "被捉住的水瓶鱼";
        cardDesc = "一只水瓶鱼，其怀孕时体内的育卵液是重要的淡水来源。";
        cardType = CardType.Creature;
        maxStackNum = 5;
        moveable = false;
        maxProductProcess = 5760;
        curProductProcess = 0;
        maxProductNum = 1;
        curProductNum = 0;
        maxFresh = 1440;
        curFresh = 1440;
        weight = 1.1f;
        events = new List<Event>();
        tags = new List<CardTag>();
        events.Add(new Event("饮用", "饮用水瓶鱼", Event_Drink, () => Judge_Drink()));
        events.Add(new Event("放生", "放生水瓶鱼", Event_Release, () => Judge_Release()));
    }

    public void Event_Drink()
    {
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Thirst, 15));
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Fullness, 4));
        TimeManager.Instance.AddTime(15);
        //TODO:删除本卡牌
        return;
    }

    public bool Judge_Drink()
    {
        if(curFresh>0)
        {
            return true;
        }
        return false;
    }

    public void Event_Release()
    {
        //TODO:删除本卡牌
        GameManager.Instance.AddCard(new AquariusFish(curProductProcess,curProductNum), true);
        return;
    }

    public bool Judge_Release()
    {
        EnvironmentBag environmentBag = GameManager.Instance.CurEnvironmentBag;        
        if(environmentBag.PlaceData.isInWater)
        {
            return true;
        }
        return false;
    }
}