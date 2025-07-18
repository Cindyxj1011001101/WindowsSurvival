/// <summary>
/// 白爆矿
/// </summary>
public class WhiteBlastMine : Card
{
    public WhiteBlastMine()
    {
        cardName = "白爆矿";
        cardDesc = "一种由白塔星早期火山运动产生的矿物，可用于消毒。直接敲碎会产生粉末和少量氧气。在适宜条件下反应可将其大部分转化为氧气。遇到明火会爆炸。";
        cardType = CardType.Resource;
        maxStackNum = 10;
        moveable = true;
        weight = 0.9f;
        events = new()
        {
            new Event("敲碎", "敲碎白爆矿", Event_Break,null)
        };
    }

    public void Event_Break()
    {
        EnvironmentBag environmentBag = GameManager.Instance.CurEnvironmentBag;
        if (environmentBag.PlaceData.isIndoor)
        {
            StateManager.Instance.OnEnvironmentChangeState(new ChangeEnvironmentStateArgs(EnvironmentStateEnum.Oxygen, 80));
        }
        else
        {
            StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Oxygen, 80));
        }
        TimeManager.Instance.AddTime(3);
    }
}