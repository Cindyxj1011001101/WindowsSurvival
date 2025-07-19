using System;

/// <summary>
/// 虹吸海葵
/// </summary>
public class Siphonophyllum : Card
{
    private Siphonophyllum()
    {
        events = new()
        {
            new Event("切割", "切割虹吸海葵", Event_Cut, Judge_Cut)
        };
    }

    private void OnProgressFull()
    {
        DestroyThis();
        GameManager.Instance.AddCard("有产物的虹吸海葵", true);
    }

    public void Event_Cut()
    {
        DestroyThis();
        var card = GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut);
        card.TryUse();
        TimeManager.Instance.AddTime(45);
        GameManager.Instance.AddCard("磁性触手", true);
        GameManager.Instance.AddCard("磁性触手", true);
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