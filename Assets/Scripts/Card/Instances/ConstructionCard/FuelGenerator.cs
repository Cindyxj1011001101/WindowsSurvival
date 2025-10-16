using System;
using System.Collections.Generic;

/// <summary>
/// 燃料发电机
/// </summary>
public class FuelGenerator : ConstructionCard
{
    private FuelStorageComponent fuelStorage; // 燃料存储组件
    private StateMachineComponent stateMachine;

    private const float ELECTRICITY_PRODUCTION = 0.8f;

    private FuelGenerator() { }

    public override void LateConstrcutor()
    {
        base.LateConstrcutor();

        // 手动添加燃料存储组件
        if (!TryGetComponent(out fuelStorage))
        {
            fuelStorage = new FuelStorageComponent(144);
            AddComponent(fuelStorage);
        }
        
        if (!TryGetComponent(out stateMachine))
        {
            var states = new List<CardState>()
            {
                new ("未点燃", "18"),
                new ("已点燃", "18", true),
            };
            stateMachine = new StateMachineComponent("未点燃", states);
            AddComponent(stateMachine);
        }

        Events = new()
        {
            new CardEvent("点燃", $"点燃{CardName}。点然后每15分钟可以产生{ELECTRICITY_PRODUCTION}单位电力。\n点燃状态下会导致室内氧气加速消耗与一氧化碳增加", Ignite, CanIgnite),
            new CardEvent("熄灭", "", Extinguish, fuelStorage.CanExtinguish),
        };
    }

    public override void Init()
    {
        base.Init();
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
        if (type != typeof(PowerNetworkFailure) || !fuelStorage.CanExtinguish(out _)) return;

        Extinguish(out _);
        ShowTip($"由于电网故障，{CardName}已熄灭并停止工作");
    }

    private void OnPowerNetworkFailureEnd(Type type)
    {
        if (type != typeof(PowerNetworkFailure)) return;

        RefreshSlot();
    }

    private bool CanIgnite(out string s)
    {
        if (GameManager.Instance.ContainsGlobalEffect<PowerNetworkFailure>())
        {
            s = $"由于电网故障，{CardName}无法为其供电";
            return false;
        }

        return fuelStorage.CanIgnite(out s);
    }

    /// <summary>
    /// 点燃时触发
    /// </summary>
    private void Ignite(out string s)
    {
        fuelStorage.Ignite(out s);

        SoundManager.Instance.PlaySound("点火_02");

        StateManager.Instance.ChangeElectricityChangeRate(ELECTRICITY_PRODUCTION);

        stateMachine.ChangeState("已点燃");
    }

    /// <summary>
    /// 熄灭时触发
    /// </summary>
    private void Extinguish(out string s)
    {
        fuelStorage.Extinguish(out s);

        // 只有玩家在同一地点时才停止音效
        if (GameManager.Instance.IsCurrentEnvironment(Bag))
            SoundManager.Instance.StopCardLoopSound(CardId);

        StateManager.Instance.ChangeElectricityChangeRate(-ELECTRICITY_PRODUCTION);

        stateMachine.ChangeState("未点燃");
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        // 添加燃料
        if (fuelStorage.CanQuickInteract(card))
        {
            tip = "添加燃料";
            return true;
        }
        // 拆毁
        return base.CanQuickInteract(card, out tip);
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        var card = slot.PeekCard();

        // 添加燃料
        if (fuelStorage.CanQuickInteract(card))
        {
            fuelStorage.QuickIneract(slot, count, out tip);
            return;
        }

        // 拆毁
        base.QuickIneract(slot, count, out tip);
    }
}