using System.Collections.Generic;

public class HumanPoweredGenerator : Card
{
    public HumanPoweredGenerator()
    {
        cardName = "人力发电机";
        cardDesc = "麦麦的父母都是香蕉寰宇联合体的人力发电工，她对这台机器可太熟了。她拼命地想摆脱这一切，可即使逃到异星还是躲不开跑轮子的命运吗？";
        cardType = CardType.Construction;
        maxStackNum = 1;
        moveable = true;
        weight = 0.9f;
        events = new List<Event>();
        events.Add(new Event("人力发电", "人力发电", Event_Generate, Judge_Generate));
        tags = new List<CardTag>();
        components = new();
    }

    public void Event_Generate()
    {
        StateManager.Instance.OnEnvironmentChangeState(new ChangeEnvironmentStateArgs(EnvironmentStateEnum.Electricity, 10));
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Thirst, -5));
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Tired, 6));
        TimeManager.Instance.AddTime(60);
    }

    public bool Judge_Generate()
    {
        if(slot.Bag is EnvironmentBag environmentBag)
        {
            if(environmentBag.EnvironmentStateDict[EnvironmentStateEnum.HasCable].curValue==1)
            {
                return true;
            }
        }
        return false;
    }
}