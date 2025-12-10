/// <summary>
/// 白爆矿堆
/// </summary>
[CardId("白爆矿堆")]
public class WhiteBlastOreStack : Card
{
    private static DropList dropList = new(
       new Drop(4, ("白爆矿", 2)),
       new Drop(8, ("白爆矿", 1)),
       new Drop(4, ("玻璃沙", 1))
       );

    protected override void RegisterCardEvents()
    {
        AddCardEvent("用铲子凿", "用铲子凿白爆矿堆", Event_Dig, Judge_Dig, () => 30);
    }

    private void DigByTool(Card tool, CardEvent e)
    {
        tool.Use();
        ApplyEventEffects(e, () =>
        {
            // 掉落卡牌(2次)
            RandomDrop(dropList, 2, () =>
            {
                PlaySound("凿_01", true);
                Use();
            });
        });
    }

    private void Event_Dig(CardEvent e)
    {
        DigByTool(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig), e);
    }

    private bool Judge_Dig(out string hint)
    {
        hint = string.Empty;
        if(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig) == null)
        {
            hint = "需要挖掘类工具";
            return false;
        }
        return true;
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        // 允许和带有切割标签的卡牌快速交互
        if (card.TryGetComponent<ToolComponent>(out var component) && component.toolTypes.Contains(ToolType.Dig))
        {
            tip = Events[0].Name;
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count)
    {
        DigByTool(slot.PeekCard(), Events[0]);
    }
}