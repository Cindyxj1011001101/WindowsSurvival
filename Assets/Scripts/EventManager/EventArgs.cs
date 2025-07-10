public enum EventType
{
    ChangeState,
    IntervalSettle,
    AddDropCard,
    ChangeLoad, // 背包载重变化
    ChangeCardProperty, // 卡牌属性变化
    Move,//场景移动界面刷新
    //RefreshCard,//结算卡牌数值刷新
    RefreshState,//更新状态数据
    ChangeDiscoveryDegree, // 探索度变化
}

public class ChangeStateArgs
{
    public StateEnum state;
    public float value;

    public ChangeStateArgs(StateEnum s, float i)
    {
        state = s;
        value = i;
    }
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

    public AddDropCardArgs(Drop d, bool b)
    {
        drop = d;
        ToPlayer = b;
    }
}

public class ChangeDiscoveryDegreeArgs
{
    public PlaceEnum place;
    public float discoveryDegree;

    public ChangeDiscoveryDegreeArgs(PlaceEnum p, float d)
    {
        place = p;
        discoveryDegree = d;
    }
}