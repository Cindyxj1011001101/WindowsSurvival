using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 有产物的爱情贝
/// </summary>
public class LoveBeadWithProduct : Card
{
    private RandomDropList dropList = new(
        new Drop(3, ("玻璃沙", 2)),
        new Drop(3, ("废金属", 2)),
        new Drop(4, ("珊瑚", 1)),
        new Drop(5, ("韧性胶管", 1)),
        new Drop(3, ("白爆矿", 1)),
        new Drop(4, ("磁性触手", 1)),
        new Drop(1, ("钢材", 1)),
        new Drop(1, ("玻璃", 1)),
        new Drop(1, ("石砖", 1)),
        new Drop(1, ("育卵液", 1)),
        new Drop(2, ("瓶装水", 1)),
        new Drop(1, ("恶臭肉", 1)),
        new Drop(1, ("小块生肉", 1)),
        new Drop(1, ("老鼠尸体", 1)),
        new Drop(1, ("水壶兰种子", 1)),
        new Drop(2, ("燃素", 1)),
        new Drop(2, ("海麻线", 2)),
        new Drop(1, ("电池", 1)),
        new Drop(1, ("海爬虫", 1)),
        new Drop(2, ("压缩饼干", 1)),
        new Drop(2, ("菱果", 1))
        );

    private LoveBeadWithProduct()
    {
        Events = new()
        {
            new CardEvent("撬开", "像开宝箱一样获得随机产物", Event_OpenByTool, Judge_OpenByTool, () => 15),
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
            tip = Events[0].name;
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        OpenByTool(slot.PeekCard(), out tip);
    }
}