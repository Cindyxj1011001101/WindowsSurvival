using System.Collections.Generic;

/// <summary>
/// 玻璃沙
/// </summary>
public class GlassSand : Card
{
    public GlassSand()
    {
        //初始化参数
        cardName = "玻璃沙";
        cardDesc = "硅基浮游藻类死亡后沉底，经长年风化形成的细腻玻璃沙。经高温锻造可制成玻璃。";
        cardType = CardType.Resource;
        maxStackNum = 10;
        moveable = true;
        weight = 0.3f;
        curEndurance = maxEndurance = 1;
        tags = new();
        events = new List<Event>();
        components = new();
    }
}