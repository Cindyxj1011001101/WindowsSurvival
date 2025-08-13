public class SeaGrass : Card
{
    private SeaGrass()
    {
        Events = new()
        {
            new Event("用手提取", "用手提取纤维", Event_CollectByHand, null, () => 30),
            new Event("用刀提取", "用刀提取纤维", Event_CollectByKnife, Judge_CollectByKnife, () => 15),
        };
    }
    public void Event_CollectByHand(out string tip)
    {
        StopUpdating();

        tip = string.Empty;
        TimeManager.Instance.AddTime(30);
        AddCard("纤维", true);

        DestroyThis();
    }
    public void Event_CollectByKnife(out string tip)
    {
        StopUpdating();

        tip = string.Empty;
        TimeManager.Instance.AddTime(15);
        AddCard("纤维", true);

        GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut).TryUse();
        DestroyThis();
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