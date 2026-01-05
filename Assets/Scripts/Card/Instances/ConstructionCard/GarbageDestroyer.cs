/// <summary>
/// 垃圾销毁器
/// </summary>
[CardId("垃圾销毁器")]
public class GarbageDestroyer : ConstructionCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("销毁", "销毁所有内容物", Event_Destroy, Judge_Destroy);
        base.RegisterCardEvents(); // 拆毁
    }

    private void Event_Destroy(CardEvent e)
    {
        var window = WindowsManager.Instance.OpenWindow("Custom", true) as CustomWindow;
        window.SetContent($"{ColorManager.Alert("内容物将被全部销毁！！")}\n确认这样做吗？");
        window.ConfirmAndCancel(() =>
        {
            PlaySound("挖掘废料_01", true);
            innerContents.Clear();
            ShowTip("内容物已完全销毁");
        });
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

    public override void QuickIneract(SlotCards slot, int count)
    {
        innerContents.QuickIneract(slot, count);
    }
}