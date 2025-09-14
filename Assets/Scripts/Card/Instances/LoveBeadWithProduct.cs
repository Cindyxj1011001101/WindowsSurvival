using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 有产物的爱情贝
/// </summary>
public class LoveBeadWithProduct : Card
{
    private RandomDropList dropList = new(
        new Drop("玻璃沙", 2, 3),
        new Drop("废金属", 2, 3),
        new Drop("珊瑚", 1, 3),
        new Drop("韧性胶管", 1, 3),
        new Drop("白爆矿", 1, 3)
        );

    private LoveBeadWithProduct()
    {
        Events = new()
        {
            new Event("撬开", "像开宝箱一样获得随机产物", Event_OpenByTool, Judge_OpenByTool, () => 15),
        };
    }

    #region 事件
    private void Event_OpenByTool(out string tip)
    {
        OpenByTool(GameManager.Instance.PlayerBag.FindCardOfToolTypes(new List<ToolType> { ToolType.Cut, ToolType.Dig }), out tip);
    }

    private bool Judge_OpenByTool(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfToolTypes(new List<ToolType> { ToolType.Cut, ToolType.Dig }) == null)
        {
            hint = "需要切割类或挖掘类工具";
            return false;
        }
        return true;
    }

    private void OpenByTool(Card tool, out string tip)
    {
        DestroyThis();
        tool.Use();

        TimeManager.Instance.AddTime(15);

        // 变回爱情贝
        TurnTo("爱情贝", Bag);

        // 随机掉落
        RandomDrop(dropList, out tip);
    }
    #endregion

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        if (card.TryGetComponent<ToolComponent>(out var component) && component.toolTypes.Intersect(new List<ToolType> { ToolType.Cut, ToolType.Dig }).Any())
        {
            tip = "撬开";
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        OpenByTool(slot.PeekCard(), out tip);
    }
}