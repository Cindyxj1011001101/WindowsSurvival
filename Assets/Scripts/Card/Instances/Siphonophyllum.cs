using System.Collections.Generic;
public class Siphonophyllum : Card
{
    public Siphonophyllum()
    {
        cardName = "虹吸海葵";
        cardDesc = "虹吸海葵";
        cardType = CardType.Creature;
        maxStackNum = 5;
        moveable = true;
        weight = 1.5f;
        events = new List<Event>()
        {
            new Event("切割", "切割虹吸海葵", Event_Cut, Judge_Cut)
        };
        components = new()
        {
            { typeof(ProgressComponent), new ProgressComponent(3600, OnProgressFull) },
        };
    }

    private void OnProgressFull()
    {
        Use();
        TryGetComponent<ProgressComponent>(out var component);
        GameManager.Instance.AddCard(new SiphonophyllumWithProduct(component.progress), true);
    }

    public void Event_Cut()
    {
        if(GetTool()==null) return;
        Use();
        GetTool().Use();
        TimeManager.Instance.AddTime(15);
        GameManager.Instance.AddCard(new MagneticTentacle(), true);
        GameManager.Instance.AddCard(new MagneticTentacle(), true);
    }
    public bool Judge_Cut()
    {
        return GetTool() != null;
    }
    public Card GetTool()
    {
        foreach (var slot in GameManager.Instance.PlayerBag.Slots)
        {
            if (slot.Cards[0].TryGetComponent<ToolComponent>(out var component) && component.toolType == ToolType.Cut)
            {
                return slot.Cards[0];
            }
        }
        return null;
    }
}