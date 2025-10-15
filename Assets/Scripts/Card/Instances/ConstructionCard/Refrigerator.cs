using System;
using System.Collections.Generic;

/// <summary>
/// 冰箱
/// </summary>
public class Refrigerator : ConstructionCard
{
    private InnerContentsComponent innerContents;
    private StateMachineComponent stateMachine;

    private const float ELECTRICITY_CONSUMPTION = 0.3f; // 每回合电力消耗

    private Refrigerator()
    {
        Events = new()
        {
            new CardEvent("开启", $"使其接入电路。接电后每15分钟消耗{ELECTRICITY_CONSUMPTION}电力，内容物腐烂速度减半", Event_TurnOn, Judge_TurnOn),
            new CardEvent("关闭", "", Event_TurnOff, Judge_TurnOff),
        };
    }

    public override void Awake()
    {
        base.Awake();

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
    protected override void Start()
    {
        EventManager.Instance.AddListener<Type>(EventType.OnGlobalEffectBegin, OnPowerNetworkFailureBegin);
        EventManager.Instance.AddListener<Type>(EventType.OnGlobalEffectEnd, OnPowerNetworkFailureEnd);
    }

    protected override void OnDestroy()
    {
        EventManager.Instance.RemoveListener<Type>(EventType.OnGlobalEffectBegin, OnPowerNetworkFailureBegin);
        EventManager.Instance.RemoveListener<Type>(EventType.OnGlobalEffectEnd, OnPowerNetworkFailureEnd);
    }

    private void OnPowerNetworkFailureBegin(Type type)
    {
        if (type != typeof(PowerNetworkFailure)) return;

        Event_TurnOff(out _);
        ShowTip($"由于电网故障，{CardName}已停止工作");
    }

    private void OnPowerNetworkFailureEnd(Type type)
    {
        if (type != typeof(PowerNetworkFailure)) return;

        RefreshSlot();
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

    /// <summary>
    /// 接电
    /// </summary>
    /// <param name="tip"></param>
    private void Event_TurnOn(out string tip)
    {
        tip = string.Empty;
        innerContents.ForEachCard(c =>
        {
            if (c.TryGetComponent<FreshnessComponent>(out var f))
            {
                f.updateRate *= .5f;
            }
        });
        stateMachine.ChangeState("已接电");
    }

    private bool Judge_TurnOn(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.ContainsGlobalEffect<PowerNetworkFailure>())
        {
            hint = $"由于电网故障，{CardName}缺少电力供应，无法开启";
            return false;
        }

        if (StateManager.Instance.Electricity.CurValue < ELECTRICITY_CONSUMPTION)
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
    private void Event_TurnOff(out string tip)
    {
        tip = string.Empty;
        innerContents.ForEachCard(c =>
        {
            if (c.TryGetComponent<FreshnessComponent>(out var f))
            {
                f.updateRate /= .5f;
            }
        });
        stateMachine.ChangeState("未接电");
    }

    private bool Judge_TurnOff(out string hint)
    {
        hint = string.Empty;
        return stateMachine.currentStateName == "已接电";
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (stateMachine.currentStateName == "未接电") return;

        if (StateManager.Instance.Electricity.CurValue < ELECTRICITY_CONSUMPTION)
        {
            Event_TurnOff(out _);
            ShowTip($"电力不足，{CardName}已停止工作");
        }
    }
}