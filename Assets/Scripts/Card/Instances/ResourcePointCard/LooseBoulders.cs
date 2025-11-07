/// <summary>
/// 松动巨石
/// </summary>
public class LooseBoulders : Card
{
    private DropList dropList = new(
       new Drop(3, ("玻璃沙", 1)),
       new Drop(2, ("白爆矿", 1)),
       new Drop(1, ("海爬虫", 1))
       );

    protected override void RegisterCardEvents()
    {
        AddCardEvent("用铲子凿", "", Event_DigByTool, Judge_DigByTool, () => 15);
    }

    protected override void OnInit()
    {
        durability.onBroken = () =>
        {
            TurnTo("从织光藻墓园到浅层岩穴", Bag);
        };
    }

    private void Event_DigByTool(out string tip)
    {
        DigByTool(GameManager.Instance.PlayerBag.FindCardOfName("钢铲"), out tip);
    }

    private bool Judge_DigByTool(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfName("钢铲") == null)
        {
            hint = "需要钢铲";
            return false;
        }
        return true;
    }

    private void DigByTool(Card tool, out string tip)
    {
        //掉落卡牌
        RandomDrop(dropList, out tip, onDrop: () =>
        {
            //消耗1点耐久度
            Use();
            // 工具消耗耐久
            tool.Use();

            ApplyEventEffects(0);
        });
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        if (card.CardId == "钢铲")
        {
            tip = Events[0].Name;
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        DigByTool(slot.PeekCard(), out tip);
    }
}
