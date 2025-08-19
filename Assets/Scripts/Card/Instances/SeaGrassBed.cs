using UnityEngine;

public class SeaGrassBed : Card
{
    private SeaGrassBed()
    {
        Events = new()
        {
            new Event("用手采集", "获得的东西更少且有可能划伤手", Event_CollectByHand, null, () => 30),
            new Event("用刀切割", "耗时更少但获得更多产物", Event_CollectByKnife, Judge_CollectByKnife, () => 15),
        };
    }

    public void Event_CollectByHand(out string tip)
    {
        Use();

        TimeManager.Instance.AddTime(30);
        RandomDropByHand(out tip);
    }

    public bool Judge_CollectByKnife(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut) == null)
        {
            hint = "需要切割类工具";
            return false;
        }
        return true;
    }

    public void Event_CollectByKnife(out string tip)
    {
        CollectByKnife(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut), out tip);
    }

    public void RandomDropByHand(out string tip)
    {
        tip = string.Empty;
        for (int i = 0; i < 2; i++)
        {
            int rand = Random.Range(0, 22);
            if (rand < 4)
            {
                AddCards("海麻线", 2, true);
            }
            else if (rand < 16)
            {
                AddCard("海麻线", true);
            }
            else if (rand < 19)
            {
                AddCard("海爬虫", true);
            }
            else if (rand < 21)
            {
                AddCard("海麻线根", true);
            }
            else
            {
                tip = "手被划伤了";
                //掉落提示："手被划伤了"
                StateManager.Instance.ChangePlayerState(PlayerStateEnum.PainLevel, 5);
                StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, -3);
            }
        }
    }
    public void RandomDropByKnife()
    {
        for (int i = 0; i < 3; i++)
        {
            int rand = Random.Range(0, 20);
            if (rand < 10)
            {
                AddCards("海麻线", 2, true);
            }
            else if (rand < 15)
            {
                AddCard("海麻线", true);
            }
            else if (rand < 18)
            {
                AddCard("海爬虫", true);
            }
            else
            {
                AddCard("海麻线根", true);
            }
        }
    }

    private void CollectByKnife(Card tool, out string tip)
    {
        Use();
        tool.Use();

        tip = string.Empty;
        TimeManager.Instance.AddTime(15);
        RandomDropByKnife();
    }

    public override bool CanQuickInteract(Card card)
    {
        // 允许和带有切割标签的卡牌快速交互
        if (card.TryGetComponent<ToolComponent>(out var component))
        {
            if (component.toolTypes.Contains(ToolType.Cut)) return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        CollectByKnife(slot.PeekCard(), out tip);
    }
}