/// <summary>
/// 海麻线
/// </summary>
public class SeaGrass : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("用手提取", "用手提取纤维", Event_CollectByHand, null, () => 30);
        AddCardEvent("用刀提取", "用刀提取纤维", Event_CollectByKnife, Judge_CollectByKnife, () => 15);
    }

    private void Event_CollectByHand(out string tip, CardEvent e)
    {
        tip = string.Empty;
        DestroyThis();
        ApplyEventEffects(e);
        AddCard("纤维", true);
    }

    private void CollectByKnife(Card tool, CardEvent e)
    {
        DestroyThis();
        tool.Use();
        ApplyEventEffects(e);
        AddCard("纤维", true);
    }

    private void Event_CollectByKnife(out string tip, CardEvent e)
    {
        tip = string.Empty;
        CollectByKnife(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut), e);
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

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        // 允许和带有切割标签的卡牌快速交互
        if (card.TryGetComponent<ToolComponent>(out var component) && component.toolTypes.Contains(ToolType.Cut))
        {
            tip = Events[1].Name;
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        tip = string.Empty;
        CollectByKnife(slot.PeekCard(), Events[1]);
    }
}