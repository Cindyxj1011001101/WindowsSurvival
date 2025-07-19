using System;

/// <summary>
/// 爱情贝
/// </summary>
public class LoveBead : Card
{
    public LoveBead()
    {
        events = new()
        {
            new Event("取贝肉", "取贝肉", Event_GetMeat, null)
        };
    }

    public void Event_GetMeat()
    {
        DestroyThis();
        TimeManager.Instance.AddTime(30);
        GameManager.Instance.AddCard("生贝肉", true);
        GameManager.Instance.AddCard("生贝肉", true);
    }

    private void OnProgressChanged()
    {
        DestroyThis();
        GameManager.Instance.AddCard("有产物的爱情贝", true);
    }

    protected override Action OnUpdate => () =>
    {
        TryGetComponent<ProgressComponent>(out var component);
        component.Update(TimeManager.Instance.SettleInterval, OnProgressChanged);
    };
}