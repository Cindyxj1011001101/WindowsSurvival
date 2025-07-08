public enum EventType
{
    ChangeState,
    IntervalSettle,
    AddDropCard,
    ChangeLoad, // 背包载重变化
    ChangeCardProperty, // 卡牌属性变化
    Move,//场景移动界面刷新
    RefreshCard//结算卡牌数值刷新
    
}

public class ChangeStateArgs
{
    public StateEnum state;
    public int value;
}

public class ChangeLoadArgs
{
    public float currentLoad;
    public float maxLoad;
}

public class AddDropCardArgs
{
    public Drop drop;
    public bool ToPlayer;

    public AddDropCardArgs(Drop d,bool b)
    {
        drop = d;
        ToPlayer = b;
    }
}