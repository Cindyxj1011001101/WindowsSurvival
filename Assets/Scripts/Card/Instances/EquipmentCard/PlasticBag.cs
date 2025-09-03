
public class PlasticBag : EquipmentCard
{
    private InnerContentsComponent innerContents;
    private PlasticBag() : base()
    {
        Events.Add(new Event("切割", "切割塑料袋", Event_Cut, Judge_Cut));
    }

    public override void LateInit()
    {
        base.LateInit();
        innerContents.weightLossRate = 0.5f; // 塑料袋的减重率
    }

    public override void OnEquipped()
    {

    }

    public override void OnUnEquipped()
    {

    }
    private void Event_Cut(out string tip)
    {
        CutThis(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut), out tip);
    }

    private bool Judge_Cut(out string hint)
    {
        hint = string.Empty;
        if (!innerContents.bag.IsEmpty)
        {
            hint = "塑料里还有东西，无法切割";
            return false;
        }
        if (GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut) == null)
        {
            hint = "需要切割类工具";
            return false;
        }
        return false;
    }

    private void CutThis(Card tool, out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        tool.Use();
        TimeManager.Instance.AddTime(15);
        AddCard("韧性胶管", true);
    }

    public override bool CanQuickInteract(Card card)
    {
        if (card.TryGetComponent<ToolComponent>(out var component))
        {
            // 如果是切割工具，并且渔获袋是空的，可以快速交互
            if (component.toolTypes.Contains(ToolType.Cut) && innerContents.bag.IsEmpty) return true;
        }
        return innerContents.CanQuickInteract(card);
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        var card = slot.PeekCard();

        // 优先切割
        if (card.TryGetComponent<ToolComponent>(out var component) && component.toolTypes.Contains(ToolType.Cut) && innerContents.bag.IsEmpty)
        {
            CutThis(card, out tip);
            return;
        }

        // 其次放入
        innerContents.QuickIneract(slot, count, out tip);
    }
}