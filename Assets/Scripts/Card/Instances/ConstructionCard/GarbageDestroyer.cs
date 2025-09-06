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
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("简单点击_01", true);
        var window = WindowsManager.Instance.OpenWindow("Confirm", true) as ConfirmWindow;
        window.SetText("确认要销毁所有内容物吗？");
        window.onConfirm = () =>
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySound("挖掘废料_01", true);
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