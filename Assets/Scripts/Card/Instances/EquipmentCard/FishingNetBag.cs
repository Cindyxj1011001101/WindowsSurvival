/// <summary>
/// 渔获袋
/// </summary>
public class FishingNetBag : EquipmentCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("切割", "切割渔获袋", Event_Cut, Judge_Cut, () => 15);
        base.RegisterCardEvents();
    }

    protected override void OnLateConstructor()
    {
        // 减重率为60%
        innerContents.weightLossRate = 0.6f;
    }

    public override void OnEquipped()
    {
    }

    public override void OnUnEquipped()
    {
    }

    private void CutThis(Card tool, CardEvent e)
    {
        tool.Use();
        ApplyEventEffects(e, () =>
        {
            DestroyThis();
            AddCard("韧性胶管", true);
            AddCards("纤维", 4, true);
        });
    }

    private void Event_Cut(CardEvent e)
    {
        CutThis(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut), e);
    }

    private bool Judge_Cut(out string hint)
    {
        hint = string.Empty;
        if (!innerContents.bag.IsEmpty)
        {
            hint = "渔获袋里还有东西，无法切割";
            return false;
        }
        if (GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut) == null)
        {
            hint = "需要切割类工具";
            return false;
        }
        return false;
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        if (card.TryGetComponent<ToolComponent>(out var component) && component.toolTypes.Contains(ToolType.Cut) && innerContents.bag.IsEmpty)
        {
            // 如果是切割工具，并且渔获袋是空的，可以快速交互
            tip = Events[0].Name;
            return true;
        }
        return innerContents.CanQuickInteract(card, out tip);
    }

    public override void QuickIneract(SlotCards slot, int count)
    {
        var card = slot.PeekCard();

        // 优先切割渔获袋
        if (card.TryGetComponent<ToolComponent>(out var component) && component.toolTypes.Contains(ToolType.Cut) && innerContents.bag.IsEmpty)
        {
            // 切割渔获袋
            CutThis(card, Events[0]);
            return;
        }

        // 其次放入渔获袋
        innerContents.QuickIneract(slot, count);
    }
}