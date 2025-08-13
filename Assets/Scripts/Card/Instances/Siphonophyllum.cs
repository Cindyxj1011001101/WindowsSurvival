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
            new Event("切割", "这会杀死虹吸海葵并获得磁性触手", Event_Cut, Judge_Cut, () => 45)
        };
    }

    private void OnProgressFull()
    {
        AddCard("有产物的虹吸海葵", Slot.Bag is PlayerBag);
        DestroyThis();
    }

    public void Event_Cut(out string tip)
    {
        StopUpdating();

        tip = string.Empty;
        TimeManager.Instance.AddTime(45);
        AddCards("磁性触手", 2, true);

        GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut).TryUse();
        DestroyThis();
    }

    public bool Judge_Cut(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut) == null)
        {
            hint = "需要切割类工具";
            return false;
        }
        return true;
    }

    protected override Action OnUpdate => () =>
    {
        TryGetComponent<ProgressComponent>(out var component);
        component.Update(TimeManager.Instance.SettleInterval, OnProgressFull);
    };
}