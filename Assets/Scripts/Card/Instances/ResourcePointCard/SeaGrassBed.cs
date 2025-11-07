/// <summary>
/// 海麻线丛
/// </summary>
public class SeaGrassBed : Card
{
    private DropList dropListHand = new(
        new Drop(4, ("海麻线", 2)),
        new Drop(12, ("海麻线", 1)),
        new Drop(3, ("海爬虫", 1)),
        new Drop(2, (out string tip) =>
        {
            tip = "手被划伤了";
            //掉落提示："手被划伤了"
            StateManager.Instance.ChangePlayerState(PlayerStateEnum.PainLevel, 5);
            StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, -3);
        })
        );

    private DropList dropListKnife = new(
        new Drop(10, ("海麻线", 2)),
        new Drop(5, ("海麻线", 1)),
        new Drop(3, ("海爬虫", 1))
        );

    protected override void RegisterCardEvents()
    {
        AddCardEvent("用手采集", "获得的东西更少且有可能划伤手", Event_CollectByHand, null, () => 30);
        AddCardEvent("用刀采集", "耗时更少但获得更多产物", Event_CollectByKnife, Judge_CollectByKnife, () => 15);
    }

    private void Event_CollectByHand(out string tip)
    {
        RandomDrop(dropListHand, out tip, 2, () =>
        {
            Use();

            ApplyEventEffects(0);
        });
    }

    private bool Judge_CollectByKnife(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut) == null)
        {
            hint = "需要切割类工具";
            return false;
        }
        return true;
    }

    private void Event_CollectByKnife(out string tip)
    {
        CollectByKnife(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut), out tip);
    }

    private void CollectByKnife(Card tool, out string tip)
    {
        RandomDrop(dropListKnife, out tip, 3, () =>
        {
            Use();
            tool.Use();

            ApplyEventEffects(1);
        });
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        // 允许和带有切割标签的卡牌快速交互
        if (card.TryGetComponent<ToolComponent>(out var component) && component.toolTypes.Contains(ToolType.Cut))
        {
            tip = Events[1].Name;
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        CollectByKnife(slot.PeekCard(), out tip);
    }
}