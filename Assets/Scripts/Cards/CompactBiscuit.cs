using System.Collections.Generic;
using UnityEngine;

//压缩饼干
public class CompactBiscuit:Card
{


    //构造函数(可增加多参构造实现非满新鲜与耐久的卡牌初始化)
    public CompactBiscuit()
    {
        //初始化参数
        cardName = "压缩饼干";
        cardDesc = "一块放了很久的饼干，年龄比麦麦的太奶奶大，但还没过保质期的一半。";
        cardImage = Resources.Load<Sprite>("CardImage/压缩饼干");
        cardType = CardType.Food;
        maxStackNum =5;
        moveable = true;
        weight = 0.3f;
        events = new List<Event>();
        events.Add(new Event("食用", "食用压缩饼干", Event_Eat, () => Judge_Eat()));
    }

    public void Event_Eat()
    {
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Fullness, 12));
        TimeManager.Instance.AddTime(3);
        //TODO:删除本卡牌
    }

    public bool Judge_Eat()
    {
        return true;
    }


}