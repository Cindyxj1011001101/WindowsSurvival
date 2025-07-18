/// <summary>
/// 瓶装水
/// </summary>
public class BottledWater : Card
{
    public BottledWater()
    {
        //初始化参数
        cardName = "瓶装水";
        cardDesc = "一瓶纯净水，连瓶子也是用水凝胶做的，饮用时连同瓶子一起喝下去。";
        cardType = CardType.Food;
        maxStackNum = 4;
        moveable = true;
        weight = 1f;
        events = new()
        {
            new Event("饮用", "饮用瓶装水", Event_Drink, null),
        };
    }

    public void Event_Drink()
    {
        DestroyThis();
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Thirst, 15));
        TimeManager.Instance.AddTime(3);
    }
}