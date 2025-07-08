public enum EventType
{
    ChangeState,
    IntervalSettle,
    AddDropCard,
    ChangeLoad, // 背包载重变化
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