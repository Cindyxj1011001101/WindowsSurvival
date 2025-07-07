public enum EventType
{
    ChangeState,
    IntervalSettle
}

public class ChangeStateArgs
{
    public StateEnum state;
    public int value;
}