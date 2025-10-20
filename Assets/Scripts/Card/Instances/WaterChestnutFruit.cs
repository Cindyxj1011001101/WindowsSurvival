/// <summary>
/// 四角菱果实
/// </summary>
public class WaterChestnutFruit : Card
{
    private WaterChestnutFruit()
    {
        Events = new()
        {
            new CardEvent("用手敲", "用手将果实敲开。将会获得四角菱果肉", Event_BreakByHand, null, () => 15,
                () => new() { { PlayerStateEnum.PainLevel, +20 } }),
            new CardEvent("用锤子敲", "用锤子将果实敲开。将会获得四角菱果肉", Event_BreakByTool, Judge_BreakByTool, () => 3),
        };
    }

    private void Event_BreakByHand(out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.PainLevel, +20);
        TimeManager.Instance.AddTime(15);
        TurnTo("菱果肉", Bag);
    }

    private void Event_BreakByTool(out string tip)
    {
        BreakByTool(GameManager.Instance.PlayerBag.FindCardOfName("钢锤"), out tip);
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

    private void BreakByTool(Card tool, out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        tool.Use();
        TimeManager.Instance.AddTime(3);
        TurnTo("菱果肉", Bag);
    }
    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        if (card.CardId == "钢锤")
        {
            tip = Events[1].name;
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        BreakByTool(slot.PeekCard(), out tip);
    }
}
