using System.Collections.Generic;
using UnityEngine;

//废金属

public class ScrapMetal:Card
{
    //构造函数(可增加多参构造实现非满新鲜与耐久的卡牌初始化)
    public ScrapMetal()
    {
        //初始化参数
        cardName = "废金属";
        cardDesc = "一块废金属，可以用来制作工具。";
        cardType = CardType.Resource;
        maxStackNum =5;
        moveable = true;
        weight = 0.6f;
        events = new List<Event>();
    }
}