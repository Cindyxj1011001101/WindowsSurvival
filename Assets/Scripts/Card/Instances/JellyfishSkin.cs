/// <summary>
/// 未处理的海蜇皮
/// </summary>
public class JellyfishSkin : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 15 },
                { PlayerStateEnum.Health, -4 },
                { PlayerStateEnum.Sanity, -5 },
                { PlayerStateEnum.Itchiness, +45 }
            },
            sound: "吃_01");
        AddCardEvent("腌渍脱毒", "", Event_Pickle, Judge_Pickle, () => 5,sound: "肉质感的卡牌拿起");
    }

    private void Pickle(Card salineWater, CardEvent e)
    {
        salineWater.DestroyThis();
        ApplyEventEffects(e, () =>
        {
            DestroyThis();
            TurnTo("腌渍中的海蜇皮", Bag);
        });
    }

    private void Event_Pickle(CardEvent e)
    {
        Pickle(GameManager.Instance.PlayerBag.FindCardOfName("盐水"), e);
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

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        // 允许和带有切割标签的卡牌快速交互
        if (card.CardId == "盐水")
        {
            tip = Events[1].Name;
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count)
    {
        Pickle(slot.PeekCard(), Events[1]);
    }
}
