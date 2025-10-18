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

    private LionJellyfishCorpse()
    {
        Events = new()
        {
            new CardEvent("用刀切割", "", Event_PeelByKnife, Judge_PeelByKnife, () => 15),
        };
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
        TimeManager.Instance.AddTime(15);
        RandomDrop(dropList, out tip);
    }
    #endregion

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        // 允许和带有切割标签的卡牌快速交互
        if (card.TryGetComponent<ToolComponent>(out var component) && component.toolTypes.Contains(ToolType.Cut))
        {
            tip = Events[0].name;
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        PeelByKnife(slot.PeekCard(), out tip);
    }
}
