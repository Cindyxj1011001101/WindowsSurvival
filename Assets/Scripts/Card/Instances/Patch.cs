/// <summary>
/// 裂缝填充物
/// </summary>
public class Patch : Card
{
    public Patch()
    {
        //初始化参数
        cardName = "裂缝填充物";
        cardDesc = "麦麦用螺丝钉将铁板钉在了大的裂缝处，剩余的缝隙用泡沫堵住。以前家里漏雨时，麦麦的父亲也曾这么做过。而现在麦麦长大了，轮到她撑起这一切了。\r\n";
        cardType = CardType.Resource;
        maxStackNum = 10;
        moveable = true;
        weight = 0.4f;
    }
}