/// <summary>
/// 爱情贝
/// </summary>
public class LoveBead : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("取贝肉", "这将会杀死爱情贝", Event_GetMeat, null, () => 30);
    }

    private void Event_GetMeat(CardEvent e)
    {
        ApplyEventEffects(e, () =>
        {
            DestroyThis();
            AddCards("生贝肉", 2, true);
        });
    }
}