/// <summary>
/// 塑料袋
/// </summary>
public class PlasticBag : EquipmentCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("切割", "切割塑料袋", Event_Cut, Judge_Cut, () => 15);
        base.RegisterCardEvents();
    }

    protected override void OnLateConstructor()
    {
        // 塑料袋的减重率为50%
        innerContents.weightLossRate = 0.5f;
    }

    public override void OnEquipped() { }

    public override void OnUnEquipped() { }

    private void CutThis(Card tool, CardEvent e)
    {
        tool.Use();
        ApplyEventEffects(e, () =>
        {
            DestroyThis();
            AddCard("韧性胶管", true);
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

        // 优先切割
        if (card.TryGetComponent<ToolComponent>(out var component) && component.toolTypes.Contains(ToolType.Cut) && innerContents.bag.IsEmpty)
        {
            CutThis(card, Events[0]);
            return;
        }

        // 其次放入
        innerContents.QuickIneract(slot, count);
    }
}