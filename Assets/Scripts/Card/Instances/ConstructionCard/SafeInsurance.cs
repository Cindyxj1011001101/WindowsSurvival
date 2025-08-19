public class SafeInsurance : Card
{
    private InnerContentsComponent innerContents;
    private SafeInsurance()
    {
        Events = new()
        {
            new Event("用手砸", "用手砸", Event_UseHand, Judge_UseHand),
            new Event("用铲子凿", "用铲子凿", Event_UseShovel, Judge_UseShovel),
            new Event("用锤子砸", "用锤子砸", Event_UseHammer, Judge_UseHammer)
        };

        AddComponent(new ConstructionComponent()
        {
        });
    }

    protected override void LateInit()
    {
        base.LateInit();
        innerContents.display = false; // 不显示内容物
    }

    public void Event_UseHand(out string tip)
    {
        tip = string.Empty;
        Use(3);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Sobriety, -5);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.PainLevel, 15);
    }

    public bool Judge_UseHand(out string hint)
    {
        hint = string.Empty;
        return true;
    }
    public void Event_UseShovel(out string tip)
    {
        UseShovel(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig), out tip);
    }

    public bool Judge_UseShovel(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig) != null)
        {
            return true;
        }
        return false;
    }
    public void Event_UseHammer(out string tip)
    {
        UseHammer(GameManager.Instance.PlayerBag.FindCardOfName("钢锤"), out tip);
    }

    public bool Judge_UseHammer(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfName("钢锤") != null)
        {
            return true;
        }
        return false;
    }

    private void OnBroken()
    {
        AddCard("被撬开的保险箱", false, out var card);
        // 继承内容物
        card.InheritComponent<InnerContentsComponent>(this);
    }

    private void UseHammer(Card tool, out string tip)
    {
        tip = string.Empty;
        Use(20, OnBroken);
        tool.Use();
        TimeManager.Instance.AddTime(15);
    }

    private void UseShovel(Card tool, out string tip)
    {
        tip = string.Empty;
        Use(8, OnBroken);
        tool.Use();
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Sobriety, -4);
        TimeManager.Instance.AddTime(15);
    }

    public override bool CanQuickInteract(Card card)
    {
        // 允许和带有切割标签的卡牌快速交互
        if (card.TryGetComponent<ToolComponent>(out var component))
        {
            if (component.toolTypes.Contains(ToolType.Dig)) return true;
        }
        return card.CardId == "钢锤";
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        base.QuickIneract(slot, count, out tip);
        var card = slot.PeekCard();
        if (slot.PeekCard().CardId == "钢锤")
        {
            UseHammer(card, out tip);
        }
        else
        {
            UseShovel(card, out tip);
        }
    }
}