/// <summary>
/// 未处理的海蜇皮
/// </summary>
public class JellyfishSkin : Card
{
    private JellyfishSkin()
    {
        Events = new()
        {
            new CardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 15 },
                { PlayerStateEnum.Health, -4 },
                { PlayerStateEnum.Sanity, -5 },
                { PlayerStateEnum.Itchiness, +45 }
            }),
            new CardEvent("腌渍脱毒", "", Event_Pickle, Judge_Pickle, () => 5),
        };
    }

    private void Event_Pickle(out string tip)
    {
        Pickle(GameManager.Instance.PlayerBag.FindCardOfName("盐水"), out tip);
    }

    private bool Judge_Pickle(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfName("盐水") == null)
        {
            hint = "需要盐水";
            return false;
        }
        return true;
    }

    private void Pickle(Card salineWater, out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        salineWater.DestroyThis();

        TimeManager.Instance.AddTime(Events[1].GetTimeEffect());
        TurnTo("腌渍中的海蜇皮", Bag);
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        // 允许和带有切割标签的卡牌快速交互
        if (card.CardId == "盐水")
        {
            tip = Events[1].name;
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        Pickle(slot.PeekCard(), out tip);
    }
}
