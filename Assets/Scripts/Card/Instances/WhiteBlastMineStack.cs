using UnityEngine;

/// <summary>
/// 白爆矿
/// </summary>
public class WhiteBlastMineStack : Card
{
    private WhiteBlastMineStack()
    {
        Events = new()
        {
            new Event("用铲子凿", "用铲子凿白爆矿堆",Event_Dig, Judge_Dig, () => 30)
        };
    }

    public void Event_Dig(out string tip)
    {
        DigByTool(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig), out tip);
    }
    public bool Judge_Dig(out string hint)
    {
        hint = string.Empty;
        if(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig) == null)
        {
            hint = "需要挖掘类工具";
            return false;
        }
        return true;
    }
    public void RandomDrop()
    {
        int rand = Random.Range(0,16);
        if (rand < 4)
        {
            AddCards("白爆矿", 2, true);
        }
        else if (rand < 12)
        {
            AddCard("白爆矿", true);
        }
        else
        {
            AddCard("玻璃沙", true);
        }
    }

    private void DigByTool(Card tool, out string tip)
    {
        Use();
        tool.Use();

        tip = string.Empty;
        TimeManager.Instance.AddTime(30);
        //掉落卡牌
        RandomDrop();
        RandomDrop();
    }

    public override bool CanQuickInteract(Card card)
    {
        // 允许和带有切割标签的卡牌快速交互
        if (card.TryGetComponent<ToolComponent>(out var component))
        {
            if (component.toolTypes.Contains(ToolType.Dig)) return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        DigByTool(slot.PeekCard(), out tip);
    }
}