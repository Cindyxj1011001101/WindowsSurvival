using System.Collections.Generic;
using UnityEngine;
public class SiphonophyllumWithProduct : Card
{
    public SiphonophyllumWithProduct()
    {
        cardName = "虹吸海葵";
        cardDesc = "虹吸海葵";
        cardType = CardType.Creature;
        maxStackNum = 5;
        moveable = true;
        weight = 1.5f;
        events = new List<Event>()
        {
            new Event("切割", "切割虹吸海葵", Event_Cut, Judge_Cut),
            new Event("采集", "采集虹吸海葵", Event_Collect, null)
        };
        components = new()
        {
            { typeof(ProgressComponent), new ProgressComponent(3600, null) },
        };
    }
    public SiphonophyllumWithProduct(int progress) : this()
    {
        TryGetComponent<ProgressComponent>(out var component);
        component.progress = progress;
    }
    public void Event_Cut()
    {
        if(GetTool()==null) return;
        Use();
        GetTool().Use();
        TimeManager.Instance.AddTime(15);
        GameManager.Instance.AddCard(new MagneticTentacle(), true);
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
    public void Event_Collect()
    {
        Use();
        GameManager.Instance.AddCard(new Siphonophyllum(), true);
        TimeManager.Instance.AddTime(15);
        int random = Random.Range(0, 6);
        if (random < 3)
        {
            GameManager.Instance.AddCard(new ScrapMetal(), true);
            GameManager.Instance.AddCard(new ScrapMetal(), true);
        }
        else if (random < 5)
        {
            GameManager.Instance.AddCard(new ScrapMetal(), true);
        }
        else
        {
            GameManager.Instance.AddCard(new MagneticTentacle(), true);
        }
    }
}