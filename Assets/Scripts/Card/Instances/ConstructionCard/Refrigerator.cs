using System.Collections.Generic;

/// <summary>
/// 冰箱
/// </summary>
public class Refrigerator : ConstructionCard
{
    private const float ELECTRICITY_CONSUMPTION = 0.3f; // 每回合电力消耗

    protected override void RegisterCardEvents()
    {
        AddCardEvent("接电", $"将其接入电网。接电后每15分钟消耗{ELECTRICITY_CONSUMPTION}电力，内容物腐烂速度减半", Event_TurnOn, Judge_TurnOn);
        AddCardEvent("断电", "", Event_TurnOff, Judge_TurnOff);
        base.RegisterCardEvents(); // 拆毁
    }

    protected override void OnLateConstructor()
    {
        var states = new List<CardState>()
        {
            new ("未接电", "16", false, true, false),
            new ("已接电", "17", false, true, true),
        };
        stateMachine = new StateMachineComponent("未接电", states);
        AddComponent(stateMachine);
    }

    protected override void OnInit()
    {
        innerContents.onAddCard = (c) =>
        {
            if (c.TryGetComponent(out FreshnessComponent f) && stateMachine.currentStateName == "已接电")
            {
                f.updateRate *= .5f;
            }
        };
        innerContents.onRemoveCard = (c) =>
        {
            if (c.TryGetComponent(out FreshnessComponent f))
            {
                f.updateRate /= .5f;
            }
        };
        EventManager.Instance.AddListener<GameEvent>(EventType.OnGameEventTrigger, OnMagneticStormBegin);
        EventManager.Instance.AddListener<GameEvent>(EventType.OnGameEventEnd, OnMagneticStormEnd);
        EventManager.Instance.AddListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnElectricityChange);
    }

    protected override void OnDestroy()
    {
        EventManager.Instance.RemoveListener<GameEvent>(EventType.OnGameEventTrigger, OnMagneticStormBegin);
        EventManager.Instance.RemoveListener<GameEvent>(EventType.OnGameEventEnd, OnMagneticStormEnd);
        EventManager.Instance.RemoveListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnElectricityChange);
    }

    private void OnMagneticStormBegin(GameEvent gameEvent)
    {
        if (gameEvent.GetType() != typeof(MagneticStorm) || stateMachine.currentStateName == "未接电") return;

        Event_TurnOff(out _, null);
        ShowTip($"受行星磁暴影响，{CardName}已断电并停止工作");
    }

    private void OnMagneticStormEnd(GameEvent gameEvent)
    {
        if (gameEvent.GetType() != typeof(MagneticStorm)) return;

        RefreshSlot();
    }

    private void OnElectricityChange(RefreshEnvironmentStateArgs args)
    {
        if (args.stateEnum != EnvironmentStateEnum.Electricity || stateMachine.currentStateName == "未接电") return;

        if (args.stateValue.GetPredictedVariableValue() < 0) // 已经接电了这里就要判断 < 0，因为 ELECTRICITY_CONSUMPTION 那部分已经包含在 GetPredictedVariableValue 里面了
        {
            Event_TurnOff(out _, null);
            ShowTip($"电力供应不足，{CardName}已断电并停止工作");
        }
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
    private void Event_TurnOn(out string tip, CardEvent e)
    {
        tip = string.Empty;
        innerContents.ForEachCard(c =>
        {
            if (c.TryGetComponent<FreshnessComponent>(out var f))
            {
                f.updateRate *= .5f;
            }
        });
        StateManager.Instance.ChangeElectricityChangeRate(-ELECTRICITY_CONSUMPTION);
        stateMachine.ChangeState("已接电");
    }

    private bool Judge_TurnOn(out string hint)
    {
        hint = string.Empty;
        if (GameEventManager.Instance.IsEventOngoing<MagneticStorm>())
        {
            hint = $"受行星磁暴影响，无法接电";
            return false;
        }

        if (StateManager.Instance.Electricity.GetPredictedVariableValue() < ELECTRICITY_CONSUMPTION)
        {
            hint = "电力供应不足";
            return false;
        }

        return stateMachine.currentStateName == "未接电";
    }

    /// <summary>
    /// 断电
    /// </summary>
    /// <param name="tip"></param>
    private void Event_TurnOff(out string tip, CardEvent e)
    {
        tip = string.Empty;
        innerContents.ForEachCard(c =>
        {
            if (c.TryGetComponent<FreshnessComponent>(out var f))
            {
                f.updateRate /= .5f;
            }
        });
        StateManager.Instance.ChangeElectricityChangeRate(ELECTRICITY_CONSUMPTION);
        stateMachine.ChangeState("未接电");
    }

    private bool Judge_TurnOff(out string hint)
    {
        hint = string.Empty;
        return stateMachine.currentStateName == "已接电";
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        if (base.CanQuickInteract(card, out tip)) return true;

        return innerContents.CanQuickInteract(card, out tip);
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        if (base.CanQuickInteract(slot.PeekCard(), out _))
        {
            base.QuickIneract(slot, count, out tip);
            return;
        }

        innerContents.QuickIneract(slot, count, out tip);
    }
}