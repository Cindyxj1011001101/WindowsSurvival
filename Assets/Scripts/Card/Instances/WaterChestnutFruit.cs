/// <summary>
/// 四角菱果实
/// </summary>
public class WaterChestnutFruit : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("用手敲", "用手将果实敲开。将会获得四角菱果肉", Event_BreakByHand, null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.PainLevel, +20 }
            },sound:"凿_01");
        AddCardEvent("用锤子敲", "用锤子将果实敲开。将会获得四角菱果肉", Event_BreakByTool, Judge_BreakByTool, () => 3,sound: "凿_01");
    }

    private void Event_BreakByHand(CardEvent e)
    {
        ApplyEventEffects(e, () =>
        {
            DestroyThis();
            TurnTo("菱果肉", Bag);
        });
    }

    private void BreakByTool(Card tool, CardEvent e)
    {
        tool.Use();
        ApplyEventEffects(e, () =>
        {
            DestroyThis();
            TurnTo("菱果肉", Bag);
        });
    }

    private void Event_BreakByTool(CardEvent e)
    {
        BreakByTool(GameManager.Instance.PlayerBag.FindCardOfName("钢锤"), e);
    }

    private bool Judge_BreakByTool(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfName("钢锤") == null)
        {
            hint = "需要钢锤";
            return false;
        }

        return true;
    }
    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        if (card.CardId == "钢锤")
        {
            tip = Events[1].Name;
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count)
    {
        BreakByTool(slot.PeekCard(), Events[1]);
    }
}
