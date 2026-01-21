/// <summary>
/// 冰箱
/// </summary>
[CardId("冰箱")]
public class Refrigerator : ConstructionCard
{
	private const float POWER_CONSUMPTION_RATE = 0.3f;	// 每回合电力消耗
	private const float DECAY_RATEE = 0.5f;				// 腐烂速率

	protected override void RegisterCardEvents()
	{
		var powerConsumptionRateText = ColorManager.ColorizeNumber(POWER_CONSUMPTION_RATE, ColorManager.Red);
		AddCardEvent("开启", $"开启{CardName}\n开启后内容物的腐烂速度变为原来的{ColorManager.ColorizePercent(DECAY_RATEE, ColorManager.Green, "0")}，" +
			$"且每{ColorManager.Colorize(15 + "分钟", ColorManager.Cyan)}消耗{powerConsumptionRateText}电力",
			powerConsumption.ConnectPower, powerConsumption.CanConnectPower);
		AddCardEvent("关闭", "", powerConsumption.DisconnectPower, powerConsumption.CanDisconnectPower);
		base.RegisterCardEvents(); // 拆毁
	}

	protected override void OnLateConstructor()
	{
		powerConsumption = new(POWER_CONSUMPTION_RATE);
		AddComponent(powerConsumption);
	}

	protected override void OnInit()
	{
		innerContents.onAddCard = (c) =>
		{
			if (c.TryGetComponent(out FreshnessComponent f) && stateMachine.currentStateName == "开启")
			{
				f.updateRate *= DECAY_RATEE;
			}
		};
		innerContents.onRemoveCard = (c) =>
		{
			if (c.TryGetComponent(out FreshnessComponent f))
			{
				f.updateRate /= DECAY_RATEE;
			}
		};
	}

	private void PowerOn()
	{
		innerContents.ForEachCard(c =>
		{
			if (c.TryGetComponent<FreshnessComponent>(out var f))
			{
				f.updateRate *= DECAY_RATEE;
			}
		});
		stateMachine.ChangeState("开启");
	}

	private void PowerOff()
	{
		innerContents.ForEachCard(c =>
		{
			if (c.TryGetComponent<FreshnessComponent>(out var f))
			{
				f.updateRate /= DECAY_RATEE;
			}
		});
		stateMachine.ChangeState("关闭");
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