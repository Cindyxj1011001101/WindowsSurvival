using System;

/// <summary>
/// 虹吸海葵
/// </summary>
public class Siphonophyllum : Card
{
    public Siphonophyllum()
    {
        cardName = "虹吸海葵";
        cardDesc = "虹吸海葵";
        cardType = CardType.Creature;
        maxStackNum = 5;
        moveable = false;
        weight = 1.5f;
        events = new()
        {
            new Event("切割", "切割虹吸海葵", Event_Cut, Judge_Cut)
        };
        components = new()
        {
            { typeof(ProgressComponent), new ProgressComponent(3600) },
        };
    }

    private void OnProgressFull()
    {
        DestroyThis();
        GameManager.Instance.AddCard(new SiphonophyllumWithProduct(), true);
    }

    public void Event_Cut()
    {
        DestroyThis();
        var card = GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut);
        card.TryUse();
        TimeManager.Instance.AddTime(45);
        GameManager.Instance.AddCard(new MagneticTentacle(), true);
        GameManager.Instance.AddCard(new MagneticTentacle(), true);
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