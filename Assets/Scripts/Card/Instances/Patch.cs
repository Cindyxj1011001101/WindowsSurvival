using System.Collections.Generic;

/// <summary>
/// 补丁
/// </summary>
public class Patch : Card
{
    public Patch()
    {
        //初始化参数
        cardName = "补丁";
        cardDesc = "用于修补各种东西。";
        cardType = CardType.Resource;
        maxStackNum = 10;
        moveable = true;
        weight = 0.4f;
        curEndurance = maxEndurance = 1;
        tags = new();
        events = new List<Event>();
        components = new();
    }
}