/// <summary>
/// 爱情贝
/// </summary>
public class LoveBead : Card
{
    private LoveBead()
    {
        Events = new()
        {
            new CardEvent("取贝肉", "这将会杀死爱情贝", Event_GetMeat, null, () => 30)
        };
    }

    private void Event_GetMeat(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        TimeManager.Instance.AddTime(30);
        AddCards("生贝肉", 2, true);
    }
}