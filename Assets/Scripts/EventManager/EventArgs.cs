public enum EventType
{
    ChangeState,
    IntervalSettle,
    AddDropCard
}

public class ChangeStateArgs
{
    public StateEnum state;
    public int value;
}