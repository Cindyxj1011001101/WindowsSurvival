using UnityEngine;

/// <summary>
/// 被安全泡沫覆盖的废料堆
/// </summary>
public class WasteHeap : Card
{
    private WasteHeap()
    {
        Events = new()
        {
            new Event("用手挖掘", "这会费时费力", Event_Dig, null, () => 45),
            new Event("用铲子挖", "比用手轻松一些", Event_DigByTool, Judge_DigByTool, () => 15),
        };
    }

    private void Event_Dig(out string tip)
    {
        //消耗1点耐久度
        Use();

        tip = string.Empty;
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("挖掘废料_01", true);

        //消耗45分钟
        TimeManager.Instance.AddTime(45);
        //掉落卡牌
        RandomDrop();
    }

    private void Event_DigByTool(out string tip)
    {
        DigByTool(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig), out tip);
    }

    private bool Judge_DigByTool(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig) == null)
        {
            hint = "需要挖掘类工具";
            return false;
        }
        return true;
    }

    private void RandomDrop()
    {
        int rand = Random.Range(0, 26);
        if (rand < 5)
        {
            AddCards("废金属", 2, true);
        }
        else if (rand < 9)
        {
            AddCard("废金属", true);
        }
        else if (rand < 11)
        {
            AddCard("韧性胶管", true);
        }
        else if (rand < 16)
        {
            AddCard("压缩饼干", true);
        }
        else if (rand < 17)
        {
            AddCard("老鼠尸体", true);
        }
        else if (rand < 18)
        {
            AddCard("腐烂物", true);
        }
        else if (rand < 20)
        {
            AddCard("氧烛", true);
        }
        else
        {
            AddCard("瓶装水", true);
        }
    }

    private void DigByTool(Card tool, out string tip)
    {
        //消耗1点耐久度
        Use();
        // 工具消耗耐久
        tool.Use();

        tip = string.Empty;
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("挖掘废料_01", true);
        //消耗15分钟
        TimeManager.Instance.AddTime(15);
        //掉落卡牌
        RandomDrop();
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        // 允许和带有切割标签的卡牌快速交互
        if (card.TryGetComponent<ToolComponent>(out var component) && component.toolTypes.Contains(ToolType.Dig))
        {
            tip = "用铲子挖";
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        DigByTool(slot.PeekCard(), out tip);
    }
}