/// <summary>
/// 压缩饼干
/// </summary>
public class CompactBiscuit : Card
{
    public CompactBiscuit()
    {
        //初始化参数
        cardName = "压缩饼干";
        cardDesc = "一块放了很久的饼干，年龄比麦麦的太奶奶大，但还没过保质期的一半。";
        cardType = CardType.Food;
        maxStackNum = 5;
        moveable = true;
        weight = 0.3f;
        events = new()
        {
            new Event("食用", "食用压缩饼干", Event_Eat, null)
        };
    }

    public void Event_Eat()
    {
        DestroyThis();
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Fullness, 12));
        TimeManager.Instance.AddTime(3);
    }
}