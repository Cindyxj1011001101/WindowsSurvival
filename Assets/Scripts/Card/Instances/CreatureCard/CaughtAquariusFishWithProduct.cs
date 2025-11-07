/// <summary>
/// 被捉住的水瓶鱼
/// </summary>
public class CaughtAquariusFishWithProduct : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("饮用", "饮用水瓶鱼的育卵液", (out string s) => EasyEvent(out s, "喝_01"), null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hydration, 40 },
                { PlayerStateEnum.Hunger, 10 }
            });

        AddCardEvent("液体装瓶", "利用凝胶装瓶器从水瓶鱼中提取育卵液，这种提取方式相对温和，不会杀死水瓶鱼。", Event_Bottling, Judge_Bottling, () => 15);

        // AddEvent("放生", "放生水瓶鱼", Event_Release, Judge_Release);
    }

    private void Event_Release(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        // 地点中增加一个有产物的水瓶鱼
        TurnTo("有产物的水瓶鱼", GameManager.Instance.CurEnvironmentBag);
    }

    private bool Judge_Release(out string hint)
    {
        hint = string.Empty;
        if (!GameManager.Instance.CurEnvironmentBag.PlaceData.isInWater)
        {
            hint = "只能放生在水域环境";
            return false;
        }
        return true;
    }

    /// <summary>
    /// 液体装瓶
    /// </summary>
    /// <param name="tip"></param>
    private void Event_Bottling(out string tip)
    {
        Bottling(GameManager.Instance.PlayerBag.FindCardOfName("凝胶装瓶器"), out tip);
    }

    private bool Judge_Bottling(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfName("凝胶装瓶器") == null)
        {
            hint = "需要凝胶装瓶器";
            return false;
        }
        return true;
    }

    /// <summary>
    /// 液体装瓶
    /// </summary>
    /// <param name="tool"></param>
    /// <param name="tip"></param>
    private void Bottling(Card tool, out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        tool.Use();

        ApplyEventEffects(1);

        TurnTo("被捉住的水瓶鱼", Bag);
        AddCard("育卵液", true);
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        if (card.CardId == "凝胶装瓶器")
        {
            tip = Events[1].Name;
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        Bottling(slot.PeekCard(), out tip);
    }
}