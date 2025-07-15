using System.Collections.Generic;
using UnityEngine;

public class RotMaterial:Card
{
    public RotMaterial()
    {
        cardName = "腐烂物";
        cardDesc = "一块腐烂物。";
        cardImage = Resources.Load<Sprite>("CardImage/腐烂物");
        cardType = CardType.Food;
        maxStackNum = 10;
        moveable = true;
        weight = 0.3f;
        events = new List<Event>();
        events.Add(new Event("食用", "食用腐烂物", Event_Eat, () => Judge_Eat()));
    }

    public void Event_Eat()
    {   
        //+6饱食
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Fullness, 6));
        //-20精神值
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.San, -20));
        //-10健康
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Health, -10));
        //消耗15分钟
        TimeManager.Instance.AddTime(15);
        //TODO:删除本卡牌
    }

    public bool Judge_Eat()
    {
        return true;
    }
    public override void Use()
    {
        return;
    }

    public override void Fresh()
    {
        return;
    }

    public override void Grow()
    {
        return;
    }
}