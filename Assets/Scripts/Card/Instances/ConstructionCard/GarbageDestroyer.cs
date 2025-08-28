/// <summary>
/// 垃圾销毁器
/// </summary>
public class GarbageDestroyer : ConstructionCard
{
    private InnerContentsComponent innerContents;
    private GarbageDestroyer()
    {
        Events = new()
        {
            new Event("销毁", "销毁所有内容物", Event_Destroy, Judge_Destroy),
        };
    }

    private void Event_Destroy(out string tip)
    {
        tip = string.Empty;
        var window = WindowsManager.Instance.OpenWindow("Confirm", true) as ConfirmWindow;
        window.SetText("确认要销毁所有内容物吗？");
        window.onConfirm = () => innerContents.Clear();
    }

    private bool Judge_Destroy(out string hint)
    {
        hint = string.Empty;
        return !innerContents.bag.IsEmpty;
    }

    public override bool CanQuickInteract(Card card)
    {
        return innerContents.CanQuickInteract(card);
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        innerContents.QuickIneract(slot, count, out tip);
    }
}