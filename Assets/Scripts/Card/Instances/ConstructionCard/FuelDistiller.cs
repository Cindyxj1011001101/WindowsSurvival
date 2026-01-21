/// <summary>
/// 燃料蒸馏器
/// </summary>
[CardId("燃料蒸馏器")]
public class FuelDistiller : ConstructionCard
{
	private const int SALINE_WATER_CONSUMPTION_RATE = 1;
	private const int FRESH_WATER_PRODUCTION_RATE = 1;

	protected override void RegisterCardEvents()
	{
		AddCardEvent("点燃", $"点燃{CardName}\n点燃后每{ColorManager.Colorize(15 + "分钟", ColorManager.Cyan)}" +
			$"消耗{ColorManager.ColorizeNumber(SALINE_WATER_CONSUMPTION_RATE, ColorManager.Red, "0")}盐水" +
			$"并产生{ColorManager.ColorizeNumber(FRESH_WATER_PRODUCTION_RATE, ColorManager.Green, "0")}淡水。" +
			$"当淡水储量达到上限时，将清空淡水存储并在内容物中生成一瓶{ColorManager.Colorize("瓶装水", ColorManager.Blue)}\n" +
			$"{ColorManager.Warning("会导致室内氧气加速消耗与一氧化碳增加")}",
			fuelStorage.Ignite, fuelStorage.CanIgnite, sound: "点火_02");
		AddCardEvent("熄灭", "", fuelStorage.Extinguish, fuelStorage.CanExtinguish, sound: "熄灭");
		AddCardEvent("倒入盐水",
			$"消耗盐水，使蒸馏器的盐水储量增加{ColorManager.ColorizeNumber(12f, ColorManager.Green, "0")}\n{ColorManager.Warning("超出盐水储量上限的部分会被浪费")}",
			Event_AddSalineWater, Judge_AddSalineWater);
		base.RegisterCardEvents(); // 拆毁
	}

	protected override void OnLateConstructor()
	{
		// 内容物不允许放入
		innerContents.allowAdd = false;
		innerContents.notAllowAddReason = "该槽位仅用于放置蒸馏产出的瓶装水";

		// 淡水存储组件
		freshWaterStorage = new(12);
		AddComponent(freshWaterStorage);

		// 盐水存储组件
		salineWaterStorage = new(24);
		AddComponent(salineWaterStorage);
	}

	protected override void OnInit()
	{
		// 取出瓶装水时，如果淡水储量达到了上限，则再生成一瓶
		innerContents.onRemoveCard = (c) =>
		{
			TryGetBottledWater();
		};
	}

	private void OnIgnite()
	{
		// 点燃后暂停所有卡牌每回合更新
		innerContents.FreezeUpdate();

		stateMachine.ChangeState("点燃");
	}

	private void OnExtinguish()
	{
		// 熄灭后恢复所有卡牌每回合更新
		innerContents.UnfreezeUpdate();

		stateMachine.ChangeState("熄灭");
	}

	private void OnBurning()
	{
		HandleDistillation();
	}

	/// <summary>
	/// 倒入盐水
	/// </summary>
	/// <param name="tip"></param>
	private void Event_AddSalineWater(CardEvent e)
	{
		AddSalineWater(GameManager.Instance.PlayerBag.FindCardOfName("盐水"));
	}

	private void AddSalineWater(Card salineWater)
	{
		salineWater.DestroyThis();
		salineWaterStorage.AddValue(12); // 盐水储量+12
	}

	private bool Judge_AddSalineWater(out string hint)
	{
		hint = string.Empty;

		if (salineWaterStorage.value >= salineWaterStorage.maxValue)
		{
			hint = "盐水储量已经达到上限";
			return false;
		}

		if (GameManager.Instance.PlayerBag.FindCardOfName("盐水") == null)
		{
			hint = "需要盐水";
			return false;
		}

		return true;
	}

	/// <summary>
	/// 处理蒸馏逻辑
	/// </summary>
	private void HandleDistillation()
	{
		if (salineWaterStorage.value < 1 || freshWaterStorage.value >= freshWaterStorage.maxValue) return;

		salineWaterStorage.AddValue(-SALINE_WATER_CONSUMPTION_RATE); // 盐水储量-1
		freshWaterStorage.AddValue(FRESH_WATER_PRODUCTION_RATE);	 // 淡水储量+1

		TryGetBottledWater();
	}

	/// <summary>
	/// 获取瓶装水
	/// </summary>
	private void TryGetBottledWater()
	{
		// 淡水储量没有达到上限，或者内容物已满，不生成瓶装水
		if (freshWaterStorage.value < freshWaterStorage.maxValue || !innerContents.bag.CanAddCard(CardFactory.GetStaticCardInstance("瓶装水"), out _)) return;

		// 淡水储量清0，生成一瓶瓶装水
		freshWaterStorage.SetValue(0);
		AddCard("瓶装水", innerContents.bag);
		ShowTip("蒸馏得到了一瓶瓶装水");
	}

	public override bool CanQuickInteract(Card card, out string tip)
	{
		// 添加燃料
		if (fuelStorage.CanQuickInteract(card))
		{
			tip = "添加燃料";
			return true;
		}

		// 放入盐水
		if (card.CardId == "盐水" && salineWaterStorage.value < salineWaterStorage.maxValue)
		{
			tip = Events[2].Name;
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

		// 放入盐水
		if (card.CardId == "盐水" && salineWaterStorage.value < salineWaterStorage.maxValue)
		{
			AddSalineWater(card);
			return;
		}

		// 拆毁
		base.QuickIneract(slot, count);
	}
}