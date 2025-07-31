using System;

/// <summary>
/// 虹吸海葵
/// </summary>
public class Siphonophyllum : Card
{
    private Siphonophyllum()
    {
        Events = new()
        {
            new Event("切割", "这会杀死虹吸海葵并获得磁性触手", Event_Cut, Judge_Cut, 45)
        };
    }

    private void OnProgressFull()
    {
        DestroyThis();
        AddCard("有产物的虹吸海葵", true);
    }

    public void Event_Cut()
    {
        DestroyThis();
        var card = GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut);
        card.TryUse();
        TimeManager.Instance.AddTime(45);
        AddCards("磁性触手", 2, true);
    }

    public bool Judge_Cut()
    {
        return GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut) != null;
    }

    protected override Action OnUpdate => () =>
    {
        TryGetComponent<ProgressComponent>(out var component);
        component.Update(TimeManager.Instance.SettleInterval, OnProgressFull);
    };
}