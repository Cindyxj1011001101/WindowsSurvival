/// <summary>
/// 海麻线
/// </summary>
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
    private void Event_CollectByHand(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        TimeManager.Instance.AddTime(30);
        AddCard("纤维", true);
    }
    private void Event_CollectByKnife(out string tip)
    {
        CollectByKnife(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut), out tip);
    }
    private bool Judge_CollectByKnife(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut) == null)
        {
            hint = "需要切割类工具";
            return false;
        }
        return true;
    }

    private void CollectByKnife(Card tool, out string tip)
    {
        DestroyThis();
        tool.Use();

        tip = string.Empty;
        TimeManager.Instance.AddTime(15);
        AddCard("纤维", true);
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        // 允许和带有切割标签的卡牌快速交互
        if (card.TryGetComponent<ToolComponent>(out var component) && component.toolTypes.Contains(ToolType.Cut))
        {
            tip = "用刀提取";
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        CollectByKnife(slot.PeekCard(), out tip);
    }
}