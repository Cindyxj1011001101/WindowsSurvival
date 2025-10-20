/// <summary>
/// 建筑卡片基类
/// </summary>
public abstract class ConstructionCard : Card
{
    private ConstructionComponent construction;
    public override void LateConstrcutor()
    {
        base.LateConstrcutor();
        TryGetComponent(out construction);
        if (construction.canBeDemolished)
        {
            Events.Add(new("暴力拆毁", $"拆毁后获得{construction.demolitionDebris}", Event_DemolishThis, Judge_DemolishThis, () => 15));
        }
    }

    /// <summary>
    /// 拆毁建筑物
    /// </summary>
    private void DemolishThis(Card tool)
    {
        if (construction == null || !construction.canBeDemolished || string.IsNullOrEmpty(construction.demolitionDebris)) return;

        // 拆毁建筑物
        DestroyThis();
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("摧毁_01", true);
        // 消耗钢锤耐久
        tool.Use();

        // 消耗15分钟
        TimeManager.Instance.AddTime(15);

        // 掉落拆毁产物
        ParseAndDrop(construction.demolitionDebris);
    }

    private void Event_DemolishThis(out string tip)
    {
        tip = string.Empty;
        DemolishThis(GameManager.Instance.PlayerBag.FindCardOfName("钢锤"));
    }

    private bool Judge_DemolishThis(out string hint)
    {
        hint = string.Empty;
        if (TryGetComponent<InnerContentsComponent>(out var innerContents) && !innerContents.bag.IsEmpty)
        {
            hint = "需要先清空内容物";
            return false;
        }

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
        // 能被拆毁并且内容物清空，可以暴力拆毁
        if (construction.canBeDemolished && card.CardId == "钢锤" &&
            (!TryGetComponent<InnerContentsComponent>(out var innerContents) || innerContents.bag.IsEmpty))
        {
            tip = "暴力拆毁";
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        tip = string.Empty;
        DemolishThis(slot.PeekCard());
    }
}