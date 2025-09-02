//变形的保险柜
public class SafeInsurance : ConstructionCard
{
    private InnerContentsComponent innerContents;
    private SafeInsurance()
    {
        Events = new()
        {
            new Event("用手砸", "如果是方块手的话或许能做到", Event_UseHand, Judge_UseHand, () => 15, () => new() { { PlayerStateEnum.Sobriety, -5 }, { PlayerStateEnum.PainLevel, 15 } }),
            new Event("用铲子凿", "还是有些费力，但是比用手好得多", Event_UseShovel, Judge_UseShovel, () => 15, () => new() { { PlayerStateEnum.Sobriety, -4 } }),
            new Event("用锤子砸", "最有效的打开保险箱的方式", Event_UseHammer, Judge_UseHammer, () => 15)
        };
    }
    public override void LateInit()
    {
        base.LateInit();
        innerContents.display = false; // 不显示内容物
        innerContents.allowAdd = innerContents.allowRemove = false; // 不允许添加或移除内容物
    }

    /// <summary>
    /// 用手砸
    /// </summary>
    /// <param name="tip"></param>
    private void Event_UseHand(out string tip)
    {
        tip = string.Empty;
        // 播放音效
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("金属受击_01", true);
        Use(3);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Sobriety, -5);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.PainLevel, 15);
        TimeManager.Instance.AddTime(15);
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
    private void Event_UseShovel(out string tip)
    {
        UseShovel(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig), out tip);
        // 播放音效
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("凿_01", true);
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
    private void Event_UseHammer(out string tip)
    {
        UseHammer(GameManager.Instance.PlayerBag.FindCardOfName("钢锤"), out tip);
        // 播放音效
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("暴力拆毁_01", true);
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
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("摧毁_01", true);
        AddCard("被撬开的保险柜", false, out var card);
        // 继承内容物
        card.InheritComponent<InnerContentsComponent>(this, out var newComponent);
        newComponent.allowAdd = newComponent.allowRemove = newComponent.display = true;
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
        // 用铲子凿
        if (card.TryGetComponent<ToolComponent>(out var component) && component.toolTypes.Contains(ToolType.Dig)) return true;

        if (card.CardId == "钢锤") return true;

        return base.CanQuickInteract(card);
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        var card = slot.PeekCard();
        if (slot.PeekCard().CardId == "钢锤")
        {
            UseHammer(card, out tip);
            return;
        }
        else if (card.TryGetComponent<ToolComponent>(out var component) && component.toolTypes.Contains(ToolType.Dig))
        {
            UseShovel(card, out tip);
            return;
        }
        base.QuickIneract(slot, count, out tip);
    }
}