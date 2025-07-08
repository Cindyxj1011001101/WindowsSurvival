public enum EventType
{
    ChangeState,
    IntervalSettle,
    AddDropCard,
    ChangeLoad, // 背包载重变化
    RefreshCard//时间间隔结算（卡牌新鲜度刷新）
}

public class ChangeStateArgs
{
    public StateEnum state;
    public int value;
}

public class ChangeLoadArgs
{
    public float currentLoad;
}

public class AddDropCardArgs
{
    public Drop drop;
    public bool ToPlayerBag;

    public AddDropCardArgs(Drop d, bool b)
    {
        drop = d;
        ToPlayerBag = b;
    }
}