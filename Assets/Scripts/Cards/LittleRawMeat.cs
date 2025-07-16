using System.Collections.Generic;
using UnityEngine;

//小块生肉
public class LittleRawMeat:Card
{
    public int maxFresh;//最大新鲜度
    public int curFresh;//当前新鲜度

    //构造函数(可增加多参构造实现非满新鲜与耐久的卡牌初始化)
    public LittleRawMeat()
    {
        //初始化参数
        cardName = "小块生肉";
        cardDesc = "一小块生肉，最好烹饪一下再食用。";
        cardImage = Resources.Load<Sprite>("CardImage/小块生肉");
        cardType = CardType.Food;
        maxStackNum =5;
        maxFresh=2880;
        curFresh=2880;
        moveable = true;
        weight = 0.5f;
        events = new List<Event>();
        events.Add(new Event("食用", "食用小块生肉", Event_Eat, () => Judge_Eat()));
    }

    public void Event_Eat()
    {
        //+12饱食
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Fullness, 12));
        //-2精神值
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.San, -2));
        //-3健康
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Health, -3));
        //消耗15分钟
        TimeManager.Instance.AddTime(15);
        //TODO:删除本卡牌
    }

    public bool Judge_Eat()
    {
        return true;
    }

    //刷新耐久度
    public override void Fresh()
    {
        curFresh--;
        if(curFresh<=0)
        {
            //TODO:删除本卡牌
            //掉落新卡牌
            GameManager.Instance.AddCard(new RotMaterial(), true);
        }
    }

}