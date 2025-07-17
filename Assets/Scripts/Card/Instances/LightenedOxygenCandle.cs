using System;
using System.Collections.Generic;

public class LightenedOxygenCandle : Card
{   
    public LightenedOxygenCandle()
    {
        cardName = "点燃的氧烛";
        cardDesc = "无需氧气助燃的化学氧烛，顶部有一个引信，按下后内部就会开始反应，在水下也能轻松点燃。";
        cardType = CardType.Tool;
        maxStackNum = 1;
        moveable = true;
        curEndurance = maxEndurance = 14;
        weight = 1.8f;
        events = new List<Event>();
        tags = new List<CardTag>();
        components = new();
    }
    protected override Action OnUpdate => () =>
    {
        Use();
        EnvironmentBag environmentBag = GameManager.Instance.CurEnvironmentBag;
        if(environmentBag.PlaceData.isIndoor)
        {
            StateManager.Instance.OnEnvironmentChangeState(new ChangeEnvironmentStateArgs(EnvironmentStateEnum.Oxygen, 10));
        }
        else
        {
            StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Oxygen, 10));
        }
    };
}