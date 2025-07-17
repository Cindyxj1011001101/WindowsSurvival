using System.Collections.Generic;

/// <summary>
/// 珊瑚
/// </summary>
public class Coral : Card
{
    public Coral()
    {
        //初始化参数
        cardName = "珊瑚";
        cardDesc = "一种疏松多孔的海洋微生物骨骼，是石灰石的原料。";
        cardType = CardType.Resource;
        maxStackNum = 10;
        moveable = true;
        weight = 0.8f;
        curEndurance = maxEndurance = 1;
        tags = new();
        events = new List<Event>();
        components = new();
    }
}