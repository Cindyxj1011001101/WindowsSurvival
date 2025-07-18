/// <summary>
/// 玻璃
/// </summary>
public class Glass : Card
{
    public Glass()
    {
        //初始化参数
        cardName = "玻璃";
        cardDesc = "玻璃";
        cardType = CardType.Resource;
        maxStackNum = 10;
        moveable = true;
        weight = 0.9f;
    }
}