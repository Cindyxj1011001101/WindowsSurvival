using System;

/// <summary>
/// 爱情贝
/// </summary>
public class LoveBead : Card
{
    private LoveBead()
    {
        Events = new()
        {
            new Event("取贝肉", "这将会杀死爱情贝", Event_GetMeat, null, () => 30)
        };
    }

    public void Event_GetMeat(out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        TimeManager.Instance.AddTime(30);
        AddCards("生贝肉", 2, true);
    }

    private void OnProgressChanged()
    {
        DestroyThis();
        AddCard("有产物的爱情贝", true);
    }

    protected override Action OnUpdate => () =>
    {
        TryGetComponent<ProgressComponent>(out var component);
        component.Update(TimeManager.Instance.SettleInterval, OnProgressChanged);
    };
}