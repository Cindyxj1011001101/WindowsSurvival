/// <summary>
/// 燃料发电机
/// </summary>
[CardId("燃料发电机")]
public class FuelGenerator : ConstructionCard
{
	private const float POWER_PRODUCTION_RATE = 0.8f;

	protected override void RegisterCardEvents()
	{
		var powerProductionRateText = ColorManager.ColorizeNumber(POWER_PRODUCTION_RATE, ColorManager.Green);
		AddCardEvent("点燃", $"点燃{CardName}\n点燃后每{ColorManager.ColorizeNumber(15, ColorManager.Cyan, "0")}分钟" +
			$"可产生{powerProductionRateText}电力\n{ColorManager.Warning("会导致室内氧气加速消耗与一氧化碳增加")}",
			fuelStorage.Ignite, CanIgnite, sound: "点火_02");
		AddCardEvent("熄灭", "", fuelStorage.Extinguish, CanExtinguish, sound: "熄灭");
		base.RegisterCardEvents(); // 拆毁
	}

	protected override void OnLateConstructor()
	{
		powerConsumption = new(-POWER_PRODUCTION_RATE);
		AddComponent(powerConsumption);
	}

	private void PowerOn()
	{
		fuelStorage.Ignite();
	}

	private void PowerOff()
	{
		fuelStorage.Extinguish();
	}

	private void OnIgnite()
	{
		powerConsumption.ConnectPower();

		stateMachine.ChangeState("点燃");
	}

	private void OnExtinguish()
	{
		// 只有玩家在同一地点时才停止音效
		if (GameManager.Instance.IsCurrentEnvironment(Bag))
			SoundManager.Instance.StopCardLoopSound(CardId);

		powerConsumption.DisconnectPower();

		stateMachine.ChangeState("熄灭");
	}

	private bool CanIgnite(out string s)
	{
		return powerConsumption.CanConnectPower(out s) && fuelStorage.CanIgnite(out s);
	}

	private bool CanExtinguish(out string s)
	{
		return powerConsumption.CanDisconnectPower(out s) && fuelStorage.CanExtinguish(out s);
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

	public override void QuickIneract(SlotCards slot, int count)
	{
		var card = slot.PeekCard();

		// 添加燃料
		if (fuelStorage.CanQuickInteract(card))
		{
			fuelStorage.QuickIneract(slot, count);
			return;
		}

		// 拆毁
		base.QuickIneract(slot, count);
	}
}