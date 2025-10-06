/// <summary>
/// 未处理的海蜇皮
/// </summary>
public class JellyfishSkin : Card
{
    private JellyfishSkin()
    {
        Events = new()
        {
            new Event("食用", "", Event_Eat, null, () => 15,
            () => new() { { PlayerStateEnum.Fullness, 15 }, { PlayerStateEnum.Health, -4 }, { PlayerStateEnum.San, -5 }, { PlayerStateEnum.Itchiness, +45 } }),
            new Event("腌渍脱毒", "", Event_Pickle, Judge_Pickle, () => 5),
        };
    }

    private void Event_Eat(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        // 播放吃的音效
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("吃_01", true);
        StateManager.Instance.ApplyPlayerStateChange(Events[0].GetPlayerEffects());
        TimeManager.Instance.AddTime(Events[0].GetTimeEffect());
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
