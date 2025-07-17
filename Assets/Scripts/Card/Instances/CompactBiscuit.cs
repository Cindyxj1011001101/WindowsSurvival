using System.Collections.Generic;

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
        //cardImage = Resources.Load<Sprite>("CardImage/压缩饼干");
        cardType = CardType.Food;
        maxStackNum = 5;
        moveable = true;
        weight = 0.3f;
        curEndurance = maxEndurance = 1;
        tags = new();
        events = new List<Event>
        {
            new Event("食用", "食用压缩饼干", Event_Eat, null)
        };
        components = new();
    }

    public void Event_Eat()
    {
        Use();
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Fullness, 12));
        TimeManager.Instance.AddTime(3);
    }
}