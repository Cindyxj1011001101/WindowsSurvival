using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 矿石释氧机
/// </summary>
public class OreReleaseOxygenMachine : ConstructionCard
{
    private const int MAX_ORE_CONSUMPTION_PROCESS = 120;    // 白爆矿消耗进度
    private const float OXYGEN_PRODUCTION_RATE = 22.5f;     // 氧气产出率
    private const int ORE_CONSUMPTION_NUM = 1;              // 白爆矿消耗量
    private const float POWER_CONSUMPTION_RATE = 0.1f;      // 电力消耗量
    private const int MAX_OXYGEN_STORAGE = 360;             // 最大氧气储量

    [JsonProperty] private int oreConsumptionProgress = MAX_ORE_CONSUMPTION_PROCESS;

    protected override void RegisterCardEvents()
    {
		AddCardEvent("接电", $"将其接入电网。接电后每15分钟产生{OXYGEN_PRODUCTION_RATE}单位氧气" +
                            $"（优先释放到当前地点，其次储存），并消耗{POWER_CONSUMPTION_RATE}单位电力\n" +
                            $"每{MAX_ORE_CONSUMPTION_PROCESS}分钟消耗{ORE_CONSUMPTION_NUM}白爆矿",
                            powerConsumption.ConnectPower, CanConnectPower);
		AddCardEvent("断电", "", powerConsumption.DisconnectPower, powerConsumption.CanDisconnectPower);
        AddCardEvent("获取氧气", $"消耗{CardName}的氧气储存，补充麦麦的氧气", oxygenStorage.Event_GetOxygen, oxygenStorage.Judge_GetOxygen);
		base.RegisterCardEvents(); // 拆毁
	}

    protected override void OnLateConstructor()
    {
        oxygenStorage = new OxygenStorageComponent(MAX_OXYGEN_STORAGE);
        AddComponent(oxygenStorage);

        var states = new List<CardState>()
        {
            new ("未接电", "0", false),
            new ("已接电", "1", true),
        };
        stateMachine = new StateMachineComponent("未接电", states);
        AddComponent(stateMachine);

        powerConsumption = new(POWER_CONSUMPTION_RATE);
        AddComponent(powerConsumption);
    }

    private bool CanConnectPower(out string hint)
    {
        if (innerContents.GetTotalCountByCardId("白爆矿") < ORE_CONSUMPTION_NUM)
        {
            hint = $"内容物中至少需要{ORE_CONSUMPTION_NUM}白爆矿";
            return false;
        }

        return powerConsumption.CanConnectPower(out hint);
	}

    private void PowerOn()
    {
		var timer = new TimerComponent(oreConsumptionProgress, MAX_ORE_CONSUMPTION_PROCESS)
		{
			tipText = "消耗矿石"
		};
		AddComponent(timer);

		stateMachine.ChangeState("已接电");

        innerContents.allowRemove = false;
        innerContents.notAllowRemoveReason = "矿石消耗中，不可取出";
	}

    private void PowerOff()
	{
		RemoveComponent<TimerComponent>();
		stateMachine.ChangeState("未接电");

        innerContents.allowRemove = true;
	}

    private bool ContentFilter(Card c, out string s)
    {
        s = string.Empty;
        if (c.CardId != "白爆矿")
        {
            s = "只能放入白爆矿";
            return false;
        }
        return true;
    }

    protected override void OnUpdate()
    {
        // 没有接电不工作
        if (!powerConsumption.Connected) return;

        // 先制氧
        GenerateOxygen();
        // 再给室内环境充气
        ReleaseOxygen();
	}

	/// <summary>
    /// 制氧
    /// </summary>
	private void GenerateOxygen()
	{
		// 增加氧气
		oxygenStorage.AddValue(OXYGEN_PRODUCTION_RATE);

		// 更新消耗矿石进度
		oreConsumptionProgress -= TimeManager.SETTLEMENT_INTERVAL;

		// 消耗矿石
		if (oreConsumptionProgress <= 0)
        {
			innerContents.DestroyCardsByCardId("白爆矿", ORE_CONSUMPTION_NUM);
            // 重置进度
            oreConsumptionProgress = MAX_ORE_CONSUMPTION_PROCESS;
		}

		// 矿石数量不够，自动断电
		if (innerContents.GetTotalCountByCardId("白爆矿") < ORE_CONSUMPTION_NUM)
		{
			powerConsumption.DisconnectPower();
			ShowTip($"白爆矿不足，{CardName}已自动断电");
		}
		// 氧气存储满了，自动断电
		else if (oxygenStorage.value >= oxygenStorage.maxValue)
		{
			powerConsumption.DisconnectPower();
			ShowTip($"氧气存储已满，{CardName}已自动断电");
		}

        // 显示进度
		if (TryGetComponent<TimerComponent>(out var timer))
		{
			timer.SetValue(oreConsumptionProgress);
		}
	}

	/// <summary>
    /// 释放氧气
    /// </summary>
	private void ReleaseOxygen()
    {
        var env = Bag as EnvironmentBag;
        // 不是室内环境不释放氧气
        if (!env.PlaceData.isIndoor) return;

        // 室内氧气剩余容量
        var remainingCapacity = env.StateDict[EnvironmentStateEnum.Oxygen].RemainingCapacity;
        // 计算释放量
        var toRelease = Mathf.Min(oxygenStorage.value, remainingCapacity);
        if (toRelease > 0)
        {
            // 释放氧气
            env.ChangeEnvironmentState(EnvironmentStateEnum.Oxygen, toRelease);
            // 氧气存量减少
            oxygenStorage.AddValue(-toRelease);
        }
	}

	public override bool CanQuickInteract(Card card, out string tip)
	{
		if (base.CanQuickInteract(card, out tip)) return true;

		return innerContents.CanQuickInteract(card, out tip);
	}

	public override void QuickIneract(SlotCards slot, int count)
	{
		if (base.CanQuickInteract(slot.PeekCard(), out _))
		{
			base.QuickIneract(slot, count);
			return;
		}

		innerContents.QuickIneract(slot, count);
	}
}