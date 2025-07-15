using System.Collections.Generic;
using UnityEngine;

//硬质纤维
public class HardFiber:Card
{


    //构造函数(可增加多参构造实现非满新鲜与耐久的卡牌初始化)
    public HardFiber()
    {
        //初始化参数
        cardName = "硬质纤维";
        cardDesc = "一块硬质纤维，可以用来制作绳索。";
        cardImage = Resources.Load<Sprite>("CardImage/硬质纤维");
        cardType = CardType.Resource;
        maxStackNum =10;
        moveable = true;
        weight = 0.25f;
        events = new List<Event>();
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