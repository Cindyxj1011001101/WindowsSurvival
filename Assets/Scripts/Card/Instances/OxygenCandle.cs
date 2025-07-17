using System.Collections.Generic;

public class OxygenCandle : Card
{
    public bool isLighted;
    public OxygenCandle()
    {
        cardName = "氧气蜡烛";
        cardDesc = "一种由白塔星早期火山运动产生的矿物，可用于消毒。直接敲碎会产生粉末和少量氧气。在适宜条件下反应可将其大部分转化为氧气。遇到明火会爆炸。";
        cardType = CardType.Tool;
        maxStackNum = 1;
        moveable = true;
        weight = 0.9f;
        isLighted=false;
        events = new List<Event>();
        events.Add(new Event("点燃", "点燃氧气蜡烛", Event_Light, Judge_Light));
        tags = new List<CardTag>();
        components = new();
    }

    public void Event_Light()
    {
        isLighted=true;
        Use();
        StateManager.Instance.OnEnvironmentChangeState(new ChangeEnvironmentStateArgs(EnvironmentStateEnum.Oxygen, 10));
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Oxygen, 10));
        TimeManager.Instance.AddTime(10);
    }

    public bool Judge_Light()
    {
        return !isLighted;
    }
}