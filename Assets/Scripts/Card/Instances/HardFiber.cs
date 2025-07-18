/// <summary>
/// 硬质纤维
/// </summary>
public class HardFiber : Card
{
    public HardFiber()
    {
        //初始化参数
        cardName = "硬质纤维";
        cardDesc = "一块硬质纤维，可以用来制作绳索。";
        cardType = CardType.Resource;
        maxStackNum = 10;
        moveable = true;
        weight = 0.25f;
    }
}