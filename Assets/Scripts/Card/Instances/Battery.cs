/// <summary>
/// 电池
/// </summary>
public class Battery : Card
{
    public Battery()
    {
        //初始化参数
        cardName = "电池";
        cardDesc = "电池";
        cardType = CardType.Resource;
        maxStackNum = 10;
        moveable = true;
        weight = 1f;
    }
}