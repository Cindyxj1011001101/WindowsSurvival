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
        var card = GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut);
        card.TryUse();
        TimeManager.Instance.AddTime(15);
        AddCard("纤维", true);
    }
    public bool Judge_CollectByKnife(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut) == null)
        {
            hint = "需要切割类工具";
            return false;
        }
        return true;
    }
}