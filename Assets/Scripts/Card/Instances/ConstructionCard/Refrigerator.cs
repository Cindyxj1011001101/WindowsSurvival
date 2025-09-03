using System.Collections.Generic;

/// <summary>
/// 冰箱
/// </summary>
public class Refrigerator : ConstructionCard
{
    private InnerContentsComponent innerContents;
    private StateMachineComponent stateMachine;

    public float electricityConsume = .3f; // 每回合电力消耗

    private Refrigerator()
    {
        Events = new()
        {
            new Event("接电", "使其接入电路。接入电路后每15分钟消耗0.3电力，冰箱里的内容物腐烂速度减半", Event_ConnectElectricity, Judge_ConnectElectricity),
            new Event("断电", "", Event_DisconnectElectricity, Judge_DisconnectElectricity),
        };
    }

    public override void LateInit()
    {
        base.LateInit();

        // 未布置和已布置两种状态
        if (!TryGetComponent(out stateMachine))
        {
            var states = new List<CardState>()
            {
                new ("未接电", "16", false, true, false),
                new ("已接电", "17", false, true, true),
            };
            stateMachine = new StateMachineComponent("未接电", states);
            AddComponent(stateMachine);
        }

        innerContents.onAddCard = (c) =>
        {
            if (c.TryGetComponent(out FreshnessComponent f) && stateMachine.currentStateName == "已接电")
            {
                f.updateRate = .5f;
            }
        };
        innerContents.onRemoveCard = (c) =>
        {
            if (c.TryGetComponent(out FreshnessComponent f))
            {
                f.updateRate = 1f;
            }
        };
    }

    private bool ContentFilter(Card c, out string s)
    {
        s = string.Empty;
        if (!c.TryGetComponent<FreshnessComponent>(out _))
        {
            s = "只能放入有新鲜度的食物";
            return false;
        }
        return true;
    }

    private void StartWorking()
    {
        foreach (var slot in innerContents.bag.Slots)
        {
            foreach (var card in slot.Cards)
            {
                if (card.TryGetComponent<FreshnessComponent>(out var f))
                {
                    f.updateRate = .5f;
                }
            }
        }
    }

    private void StopWorking()
    {
        foreach (var slot in innerContents.bag.Slots)
        {
            foreach (var card in slot.Cards)
            {
                if (card.TryGetComponent<FreshnessComponent>(out var f))
                {
                    f.updateRate = 1f;
                }
            }
        }
    }

    /// <summary>
    /// 接电
    /// </summary>
    /// <param name="tip"></param>
    private void Event_ConnectElectricity(out string tip)
    {
        tip = string.Empty;
        stateMachine.ChangeState("已接电");
        StartWorking();
    }

    private bool Judge_ConnectElectricity(out string hint)
    {
        hint = string.Empty;

        if (StateManager.Instance.Electricity.CurValue < electricityConsume)
        {
            hint = "电力不足";
            return false;
        }

        return stateMachine.currentStateName == "未接电";
    }

    /// <summary>
    /// 断电
    /// </summary>
    /// <param name="tip"></param>
    private void Event_DisconnectElectricity(out string tip)
    {
        tip = string.Empty;
        stateMachine.ChangeState("未接电");
        StopWorking();
    }

    private bool Judge_DisconnectElectricity(out string hint)
    {
        hint = string.Empty;
        return stateMachine.currentStateName == "已接电";
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (stateMachine.currentStateName == "未接电") return;

        if (StateManager.Instance.Electricity.CurValue < electricityConsume)
        {
            stateMachine.ChangeState("未接电");
            StopWorking();
            ShowTip("电力不足，冰箱已自动断电");
        }
    }
}