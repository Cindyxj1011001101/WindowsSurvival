/// <summary>
/// 垃圾销毁器
/// </summary>
public class GarbageDestroyer : ConstructionCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("销毁", "销毁所有内容物", Event_Destroy, Judge_Destroy);
        base.RegisterCardEvents(); // 拆毁
    }

    private void Event_Destroy(out string tip, CardEvent e)
    {
        tip = string.Empty;
        var window = WindowsManager.Instance.OpenWindow("Confirm", true) as ConfirmWindow;
        window.SetContent("确认要销毁所有内容物吗？");
        window.onConfirm = () =>
        {
            PlaySound("挖掘废料_01", true);
            innerContents.Clear();
        };
    }

    private bool Judge_Destroy(out string hint)
    {
        hint = string.Empty;
        return !innerContents.bag.IsEmpty;
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        return innerContents.CanQuickInteract(card, out tip);
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        innerContents.QuickIneract(slot, count, out tip);
    }
}