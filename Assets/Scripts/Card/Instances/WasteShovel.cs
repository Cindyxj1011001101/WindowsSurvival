public class WasteShovel : Card
{
    private WasteShovel()
    {
        Events = new()
        {
            new Event("使用", "使用铲子", Event_Use, null)
        };
    }
    //交互行为：使用
    public void Event_Use()
    {
    }
}