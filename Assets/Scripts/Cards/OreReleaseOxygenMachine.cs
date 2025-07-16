using System.Collections.Generic;
using UnityEngine;

public class OreReleaseOxygenMachine : Card
{
    public bool isWorking;//是否已打开
    public int maxOxygenStorage;//最大氧气存储
    public int curOxygenStorage;//当前氧气存储数量
    public int maxTimeProgress;//最大时间进度
    public int curTimeProgress;//当前时间进度
    

    public OreReleaseOxygenMachine()
    {
        cardName = "矿石释氧机";
        cardDesc = "将白爆矿加热到合适的温度使其反应释放氧气。操作过程中请勿吸烟。";
        cardImage = Resources.Load<Sprite>("CardImage/矿石释氧机");
        cardType = CardType.Construction;
        maxStackNum = 1;
        moveable = false;
        weight = 1.1f;
        maxOxygenStorage = 320;
        curOxygenStorage = 0;
        maxTimeProgress = 3600;
        curTimeProgress = 0;
        events = new List<Event>();
        events.Add(new Event("打开", "打开矿石释氧机", Event_Open, () => Judge_Open()));
        events.Add(new Event("关闭", "关闭矿石释氧机", Event_Close, () => Judge_Close()));
        events.Add(new Event("获取氧气", "获取氧气", Event_GetOxygen, () => Judge_GetOxygen()));
        tags = new List<CardTag>();
    }

    public void Event_Open()
    {
        isWorking = true;
    }

    public bool Judge_Open()    
    {
        return !isWorking;
    }

    public void Event_Close()
    {
        isWorking = false;
    }

    public bool Judge_Close()
    {
        return isWorking;
    }       

    public void Event_GetOxygen()
    {
        //获取氧气逻辑，将氧气存储到氧气罐中，不够放的不溢出
    }

    public bool Judge_GetOxygen()
    {
        //TODO:判断是否装备氧气罐，氧气罐是否已满
        return false;
    }
   
    public override void TimeProgress()
    {
        if(isWorking)
        {
            curTimeProgress+=15;
            if(curTimeProgress>=maxTimeProgress)
            {
                curTimeProgress = 0;
                //TODO：需要判断是否只能放在环境中？
            }
        }
    }
}