using System.Collections.Generic;

/// <summary>
/// 瓶装水
/// </summary>
public class BottledWater : Card
{
    public BottledWater()
    {
        //初始化参数
        cardName = "瓶装水";
        cardDesc = "一瓶纯净水，连瓶子也是用水凝胶做的，饮用时连同瓶子一起喝下去。";
        cardType = CardType.Food;
        maxStackNum = 4;
        moveable = true;
        weight = 1f;
        curEndurance = maxEndurance = 1;
        tags = new();
        events = new List<Event>
        {
            new Event("饮用", "饮用瓶装水", Event_Drink, null),
        };
        components = new();
    }

    public void Event_Drink()
    {
        Use();
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Thirst, 15));
        TimeManager.Instance.AddTime(3);
    }
}