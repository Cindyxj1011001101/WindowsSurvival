public enum EventType
{
    ChangeState
}

public class ChangeStateArgs
{
    public StateEnum state;
    public int value;
}