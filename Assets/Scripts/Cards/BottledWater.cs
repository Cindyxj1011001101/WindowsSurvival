using System.Collections.Generic;
using UnityEngine;

//瓶装水
public class BottledWater : Card//或许要改成英文名
{


    //构造函数(可增加多参构造实现非满新鲜与耐久的卡牌初始化)
    public BottledWater()
    {
        //初始化参数
        cardName = "瓶装水";
        cardDesc = "一瓶纯净水，连瓶子也是用水凝胶做的，饮用时连同瓶子一起喝下去。";
        cardImage = Resources.Load<Sprite>("CardImage/瓶装水");
        cardType = CardType.Food;
        maxStackNum = 4;
        moveable = true;
        weight = 1f;
        events = new List<Event>();
        events.Add(new Event("饮用", "饮用瓶装水", Event_Drink, () => Judge_Drink()));
    }

    public void Event_Drink()
    {
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Thirst, 1));
        TimeManager.Instance.AddTime(3);
        //TODO:删除本卡牌
    }

    public bool Judge_Drink()
    {
        return true;
    }

    public override void Use()
    {
        return;
    }

    public override void Grow()
    {
        return;
    }

    public override void Fresh()
    {
        return;
    }
}