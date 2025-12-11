/// <summary>
/// 变形的保险柜
/// </summary>
[CardId("变形的保险柜")]
public class SafeInsurance : ConstructionCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("用手砸", "如果是方块手的话或许能做到", Event_UseHand, Judge_UseHand,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Sobriety, -5 },
                { PlayerStateEnum.PainLevel, 15 }
            });
        AddCardEvent("用铲子凿", "还是有些费力，但是比用手好得多", Event_UseShovel, Judge_UseShovel,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Sobriety, -4 }
            });
        AddCardEvent("用锤子砸", "最有效的打开保险箱的方式", Event_UseHammer, Judge_UseHammer, () => 15);
        base.RegisterCardEvents(); // 拆毁
    }

    protected override void OnLateConstructor()
    {
        innerContents.display = false; // 不显示内容物
        innerContents.allowAdd = innerContents.allowRemove = false; // 不允许添加或移除内容物
    }

    protected override void OnInit()
    {
        durability.onBroken = OnBroken;
    }

    /// <summary>
    /// 用手砸
    /// </summary>
    /// <param name="tip"></param>
    private void Event_UseHand(CardEvent e)
    {
        ApplyEventEffects(e, () =>
        {
            PlaySound("金属受击_01", true);
            Use(3);
        });
    }

    private bool Judge_UseHand(out string hint)
    {
        hint = string.Empty;
        return true;
    }

    /// <summary>
    /// 用铲子凿
    /// </summary>
    /// <param name="tip"></param>
    private void UseShovel(Card tool, CardEvent e)
    {
        tool.Use();
        ApplyEventEffects(e, () =>
        {
            PlaySound("凿_01", true);
            Use(8);
        });
    }

    private void Event_UseShovel(CardEvent e)
    {
        UseShovel(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig), e);
    }

    private bool Judge_UseShovel(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig) == null)
        {
            hint = "需要挖掘类工具";
            return false;
        }
        return true;
    }

    /// <summary>
    /// 用锤子砸
    /// </summary>
    /// <param name="tip"></param>
    private void UseHammer(Card tool, CardEvent e)
    {
        tool.Use();
        ApplyEventEffects(e, () =>
        {
            PlaySound("暴力拆毁_01", true);
            Use(20);
        });
    }

    private void Event_UseHammer(CardEvent e)
    {
        UseHammer(GameManager.Instance.PlayerBag.FindCardOfName("钢锤"), e);
    }

    private bool Judge_UseHammer(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfName("钢锤") == null)
        {
            hint = "需要钢锤";
            return false;
        }
        return true;
    }

    private void OnBroken()
    {
        // 播放音效
        PlaySound("摧毁_01", true);
        var card = CardFactory.CreateCard("被撬开的保险柜");
        // 继承内容物
        card.InheritComponent<InnerContentsComponent>(this, out var newComponent);
        newComponent.allowAdd = newComponent.allowRemove = newComponent.display = true;
        TurnTo(card, Bag);
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;

        // 用铲子凿
        if (card.TryGetComponent<ToolComponent>(out var component) && component.toolTypes.Contains(ToolType.Dig))
        {
            tip = Events[1].Name;
            return true;
        }

        if (card.CardId == "钢锤")
        {
            tip = Events[2].Name;
            return true;
        }

        return false;
    }

    public override void QuickIneract(SlotCards slot, int count)
    {
        var card = slot.PeekCard();

        if (card.TryGetComponent<ToolComponent>(out var component) && component.toolTypes.Contains(ToolType.Dig))
        {
            UseShovel(card, Events[1]);
            return;
        }

        if (slot.PeekCard().CardId == "钢锤")
        {
            UseHammer(card, Events[2]);
            return;
        }
    }
}