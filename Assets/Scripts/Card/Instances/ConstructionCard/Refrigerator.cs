using System.Collections.Generic;

/// <summary>
/// 冰箱
/// </summary>
[CardId("冰箱")]
public class Refrigerator : ConstructionCard
{
    private const float POWER_CONSUMPTION_RATE = 0.3f; // 每回合电力消耗

    protected override void RegisterCardEvents()
    {
        AddCardEvent("接电", $"将其接入电网。接电后内容物的腐烂速度减半，并且每15分钟消耗{POWER_CONSUMPTION_RATE}电力", powerConsumption.ConnectPower, powerConsumption.CanConnectPower);
        AddCardEvent("断电", "", powerConsumption.DisconnectPower, powerConsumption.CanDisconnectPower);
        base.RegisterCardEvents(); // 拆毁
    }

    protected override void OnLateConstructor()
    {
        var states = new List<CardState>()
        {
            new ("未接电", "16", false),
            new ("已接电", "17", false),
        };
        stateMachine = new StateMachineComponent("未接电", states);
        AddComponent(stateMachine);

        powerConsumption = new(POWER_CONSUMPTION_RATE);
        AddComponent(powerConsumption);
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
    }

    private void PowerOn()
    {
		innerContents.ForEachCard(c =>
		{
			if (c.TryGetComponent<FreshnessComponent>(out var f))
			{
				f.updateRate *= .5f;
			}
		});
		stateMachine.ChangeState("已接电");
	}

    private void PowerOff()
    {
		innerContents.ForEachCard(c =>
		{
			if (c.TryGetComponent<FreshnessComponent>(out var f))
			{
				f.updateRate /= .5f;
			}
		});
		stateMachine.ChangeState("未接电");
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