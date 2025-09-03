using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CoralReef : Card
{
    private CoralReef()
    {
        Events = new()
        {
            new Event("用铲子凿", "用铲子凿珊瑚礁", Event_Dig, Judge_Dig, () => 45),
            new Event("欣赏", "一天内多次欣赏获得的数值会衰减", Event_Enjoy, null,() => 15,
            () => new Dictionary<PlayerStateEnum, float>() { { PlayerStateEnum.San, 6 * GlobalDataManager.Instance.saveData.GetReduce(CardId) }, { PlayerStateEnum.Sobriety, 4 * GlobalDataManager.Instance.saveData.GetReduce(CardId)} })
        };
    }
    public override void LateInit()
    {
        base.LateInit();
        if (!GlobalDataManager.Instance.saveData.ReduceActionDict.ContainsKey(CardId))
        {
            GlobalDataManager.Instance.saveData.ReduceActionDict.Add(CardId,
                new Reduce()
                {
                    maxReduceCount = 2,
                    curReduceCount = 0,
                    reduceRate = 0.5f
                });
        }

    }
    private void Event_Dig(out string tip)
    {
        DigByTool(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig), out tip);
    }

    private bool Judge_Dig(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig) == null)
        {
            hint = "需要挖掘类工具";
            return false;
        }
        return true;
    }

    private void Event_Enjoy(out string tip)
    {
        tip = string.Empty;
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, 6 *GlobalDataManager.Instance.saveData.GetReduce(CardId));
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Sobriety, 4 *GlobalDataManager.Instance.saveData.GetReduce(CardId));
        GlobalDataManager.Instance.saveData.AddCardReduce(CardId);
        TimeManager.Instance.AddTime(15);
    }
    
    private void RandomDrop()
    {
        int rand = Random.Range(0, 45);
        if (rand < 30)
        {
            AddCard("珊瑚", true);
        }
        else if (rand < 38)
        {
            AddCard("海爬虫", true);
        }
        else if (rand < 43)
        {
            AddCard("白爆矿", true);
        }
        else
        {
            AddCard("有产物的水瓶鱼", false);
        }
    }

    private void DigByTool(Card tool, out string tip)
    {
        tool.Use();

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("挖掘废料_01", true);
        tip = string.Empty;
        TimeManager.Instance.AddTime(45);
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
    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (TimeManager.Instance.AnotherDay())
        {
            RefreshSlot();
        }
    }
}