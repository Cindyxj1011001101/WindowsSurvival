using System.Collections.Generic;

/// <summary>
/// 腐烂物
/// </summary>
public class RotMaterial : Card
{
    public RotMaterial()
    {
        cardName = "腐烂物";
        cardDesc = "一块腐烂物。";
        cardType = CardType.Food;
        maxStackNum = 10;
        moveable = true;
        weight = 0.3f;
        curEndurance = maxEndurance = 1;
        tags = new()
        {
            CardTag.Rubbish,
        };
        events = new List<Event>
        {
            new Event("食用", "食用腐烂物", Event_Eat, null)
        };
        components = new();
    }

    public void Event_Eat()
    {
        Use();
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
}