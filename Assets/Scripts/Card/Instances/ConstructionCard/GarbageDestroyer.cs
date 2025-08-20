/// <summary>
/// 垃圾销毁器
/// </summary>
public class GarbageDestroyer : Card
{
    private InnerContentsComponent innerContents;
    private GarbageDestroyer()
    {
        Events = new()
        {
            new Event("销毁垃圾", "销毁垃圾", Event_Destroy, Judge_Destroy),
        };
    }

    private void Event_Destroy(out string tip)
    {
        tip = string.Empty;
        innerContents.bag.Clear();
        EventManager.Instance.TriggerEvent(EventType.ChangeCardProperty, this);
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