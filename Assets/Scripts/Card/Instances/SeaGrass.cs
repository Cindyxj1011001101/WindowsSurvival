using UnityEngine;
public class SeaGrass : Card
{
    private SeaGrass()
    {
        Events = new()
        {
            new Event("用手提取", "用手提取海麻线", Event_CollectByHand, null),
            new Event("用刀提取", "用刀提取海麻线", Event_CollectByKnife, Judge_CollectByKnife),
        };
    }
    public void Event_CollectByHand(out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        TimeManager.Instance.AddTime(30);
        AddCard("纤维", true);
    }
    public void Event_CollectByKnife(out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        TimeManager.Instance.AddTime(15);
        var card = GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut);
        card.TryUse();
        AddCard("纤维", true);
    }
    public bool Judge_CollectByKnife()
    {
        return GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut) != null;
    }

}