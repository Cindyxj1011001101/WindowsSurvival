public class OpenedInsurance : Card
{
    private InnerContentsComponent innerContents;
    private OpenedInsurance()
    {
        Events = new()
        {
            new Event("完整拆卸", "完整拆卸", Event_CompleteTearDown, Judge_CompleteTearDown),
            new Event("暴力拆毁", "暴力拆毁", Event_ViolentTearDown, Judge_ViolentTearDown),
        };
    }
    private void Event_CompleteTearDown(out string tip)
    {
        tip = string.Empty;
        GameManager.Instance.PlayerBag.FindCardOfName("精密扳手").Use();
        DestroyThis();
        AddCard("建筑工程包(被撬开的保险柜)",true);
        TimeManager.Instance.AddTime(45);
    }

    private bool Judge_CompleteTearDown(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfName("精密扳手")!=null)
        {
            return true;
        }
        return false;
    }
    private void Event_ViolentTearDown(out string tip)
    {
        tip = string.Empty;
        GameManager.Instance.PlayerBag.FindCardOfName("钢锤").Use();
        DestroyThis();
        AddCard("钢材", true);
        TimeManager.Instance.AddTime(15);
        
    }

    private bool Judge_ViolentTearDown(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfName("钢锤")!=null)
        {
            return true;
        }
        return false;
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