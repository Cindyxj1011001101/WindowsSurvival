/// <summary>
/// 有产物的虹吸海葵
/// </summary>
public class SiphonophyllumWithProduct : Card
{
    private RandomDropList dropList = new(
       new Drop("废金属", 2, 3),
       new Drop("废金属", 1, 2),
       new Drop("磁性触手", 1, 1)
       );

    private SiphonophyllumWithProduct()
    {
        Events = new()
        {
            new Event("切割", "这会杀死虹吸海葵并获得磁性触手", Event_Cut, Judge_Cut, () =>45),
            new Event("采集", "虹吸海葵上似乎富集了很多金属", Event_Collect, null, () => 15)
        };
    }

    private void Event_Cut(out string tip)
    {
        Cut(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut), out tip);
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

    private void Event_Collect(out string tip)
    {
        DestroyThis();

        TimeManager.Instance.AddTime(15);

        // 变回虹吸海葵
        TurnTo("虹吸海葵", Bag);

        RandomDrop(dropList, out tip);
    }

    private void Cut(Card tool, out string tip)
    {
        DestroyThis();
        tool.Use();

        tip = string.Empty;
        TimeManager.Instance.AddTime(45);
        AddCards("磁性触手", 3, true);
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        // 允许和带有切割标签的卡牌快速交互
        if (card.TryGetComponent<ToolComponent>(out var component) && component.toolTypes.Contains(ToolType.Cut))
        {
            tip = "切割";
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        Cut(slot.PeekCard(), out tip);
    }
}