/// <summary>
/// 有产物的虹吸海葵
/// </summary>
public class SiphonophyllumWithProduct : Card
{
    private static DropList dropList = new(
       new Drop(3, ("废金属", 2)),
       new Drop(2, ("废金属", 1)),
       new Drop(3, ("磁性触手", 1))
       );

    protected override void RegisterCardEvents()
    {
        AddCardEvent("切割", "这会杀死虹吸海葵并获得磁性触手", Event_Cut, Judge_Cut, () => 45);
        AddCardEvent("采集", "虹吸海葵上似乎富集了很多金属", Event_Collect, null, () => 15);
    }

    private void Cut(Card tool, CardEvent e)
    {
        tool.Use();
        ApplyEventEffects(e, () =>
        {
            DestroyThis();
            AddCards("磁性触手", 3, true);
        });
    }

    private void Event_Cut(CardEvent e)
    {
        Cut(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut), e);
    }

    private bool Judge_Cut(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut) == null)
        {
            hint = "需要切割类工具";
            return false;
        }
        return true;
    }

    private void Event_Collect(CardEvent e)
    {
        ApplyEventEffects(e, () =>
        {
            DestroyThis();
            // 变回虹吸海葵
            TurnTo("虹吸海葵", Bag);
            RandomDrop(dropList);
        });
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        // 允许和带有切割标签的卡牌快速交互
        if (card.TryGetComponent<ToolComponent>(out var component) && component.toolTypes.Contains(ToolType.Cut))
        {
            tip = Events[0].Name;
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count)
    {
        Cut(slot.PeekCard(), Events[0]);
    }
}