using System.Collections.Generic;

/// <summary>
/// 废金属
/// </summary>
public class ScrapMetal : Card
{
    public ScrapMetal()
    {
        //初始化参数
        cardName = "废金属";
        cardDesc = "一块废金属，可以用来制作工具。";
        cardType = CardType.Resource;
        maxStackNum = 5;
        moveable = true;
        weight = 0.6f;
        curEndurance = maxEndurance = 1;
        tags = new();
        events = new List<Event>();
        components = new();
    }
}