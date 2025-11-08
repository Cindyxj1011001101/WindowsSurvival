/// <summary>
/// 建筑卡片基类
/// </summary>
public abstract class ConstructionCard : Card
{
    private const int DEMOLITION_TIME = 15;

    protected override void RegisterCardEvents()
    {
        if (construction.canBeDemolished)
        {
            AddCardEvent("暴力拆毁", $"拆毁后获得{construction.demolitionDrops}", Event_DemolishThis, Judge_DemolishThis, () => DEMOLITION_TIME);
        }
    }

    /// <summary>
    /// 拆毁建筑物
    /// </summary>
    public void DemolishThis(Card tool)
    {
        // 拆毁音效
        PlaySound("摧毁_01", true);

        // 拆毁建筑物
        DestroyThis();
        // 消耗钢锤耐久
        tool?.Use();

        // 消耗15分钟
        TimeManager.Instance.AddTime(DEMOLITION_TIME);

        // 掉落拆毁产物
        ParseAndDrop(construction.demolitionDrops);
    }

    private void Event_DemolishThis(out string tip, CardEvent e)
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
        if (construction.canBeDemolished &&
            card.CardId == "钢锤" &&
            (!TryGetComponent<InnerContentsComponent>(out var innerContents) || innerContents.bag.IsEmpty))
        {
            // 暴力拆毁是最后的交互事件
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