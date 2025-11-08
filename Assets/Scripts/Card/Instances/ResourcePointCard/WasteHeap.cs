/// <summary>
/// 被安全泡沫覆盖的废料堆
/// </summary>
public class WasteHeap : Card
{
    private DropList dropList = new(
       new Drop(5, ("废金属", 2)),
       new Drop(4, ("废金属", 1)),
       new Drop(4, ("韧性胶管", 1)),
       new Drop(3, ("压缩饼干", 1)),
       new Drop(1, ("老鼠尸体", 1)),
       new Drop(1, ("腐烂物", 1)),
       new Drop(2, ("氧烛", 1))
       );

    protected override void RegisterCardEvents()
    {
        AddCardEvent("用手挖掘", "这会费时费力", Event_Dig, null, () => 45);
        AddCardEvent("用铲子挖", "比用手轻松一些", Event_DigByTool, Judge_DigByTool, () => 15);
    }

    private void Event_Dig(out string tip, CardEvent e)
    {
        PlaySound("挖掘废料_01", true);
        //掉落卡牌
        RandomDrop(dropList, out tip, onDrop: () =>
        {
            Use();
            ApplyEventEffects(e);
        });
    }

    private void DigByTool(Card tool, out string tip, CardEvent e)
    {
        PlaySound("挖掘废料_01", true);
        //掉落卡牌
        RandomDrop(dropList, out tip, onDrop: () =>
        {
            Use();
            tool.Use();
            ApplyEventEffects(e);
        });
    }

    private void Event_DigByTool(out string tip, CardEvent e)
    {
        DigByTool(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig), out tip, e);
    }

    private bool Judge_DigByTool(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig) == null)
        {
            hint = "需要挖掘类工具";
            return false;
        }
        return true;
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        if (card.TryGetComponent<ToolComponent>(out var component) && component.toolTypes.Contains(ToolType.Dig))
        {
            tip = Events[1].Name;
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        DigByTool(slot.PeekCard(), out tip, Events[1]);
    }
}