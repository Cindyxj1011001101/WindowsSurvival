/// <summary>
/// 狮子水母尸体
/// </summary>
public class LionJellyfishCorpse : Card
{
    private DropList dropList = new(
       new Drop(2, ("未处理的海蜇皮", 1), ("盐水", 1)),
       new Drop(2, ("未处理的海蜇皮", 2)),
       new Drop(1, ("未处理的海蜇皮", 1))
       );

    protected override void RegisterCardEvents()
    {
        AddCardEvent("用刀切割", "", Event_PeelByKnife, Judge_PeelByKnife, () => 15);
        AddCardEvent("咬一口", "", Event_Bite, null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, +8 },
                { PlayerStateEnum.Health, -4 },
                { PlayerStateEnum.Sanity, -5 },
                { PlayerStateEnum.Itchiness, +45 },
            });
    }

    #region 用刀切割
    private void Event_PeelByKnife(out string tip)
    {
        PeelByKnife(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut), out tip);
    }

    private bool Judge_PeelByKnife(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut) == null)
        {
            hint = "需要切割类工具";
            return false;
        }
        return true;
    }

    private void PeelByKnife(Card knife, out string tip)
    {
        Use();
        knife.Use();

        //消耗15分钟
        ApplyEventEffects(0);
        RandomDrop(dropList, out tip);
    }
    #endregion

    /// <summary>
    /// 咬一口
    /// </summary>
    /// <param name="tip"></param>
    private void Event_Bite(out string tip)
    {
        tip = string.Empty;
        Use();
        ApplyEventEffects(1);
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        // 允许和带有切割标签的卡牌快速交互
        if (card.TryGetComponent<ToolComponent>(out var component) && component.toolTypes.Contains(ToolType.Cut))
        {
            tip = Events[0].Name;
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        PeelByKnife(slot.PeekCard(), out tip);
    }
}
