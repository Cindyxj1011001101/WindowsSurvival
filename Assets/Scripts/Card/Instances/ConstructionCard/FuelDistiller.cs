using System.Collections.Generic;

/// <summary>
/// 燃料蒸馏器
/// </summary>
public class FuelDistiller : ConstructionCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("点燃", "点燃蒸馏器。将盐水蒸馏成淡水。\n点燃状态下会导致室内氧气加速消耗与一氧化碳增加", Ignite, fuelStorage.CanIgnite);
        AddCardEvent("熄灭", "", Extinguish, fuelStorage.CanExtinguish);
        AddCardEvent("倒入盐水", "消耗盐水，使蒸馏器的盐水储量+12\n！可能会造成浪费！", Event_AddSalineWater, Judge_AddSalineWater);
        base.RegisterCardEvents(); // 拆毁
    }

    protected override void OnLateConstructor()
    {
        // 手动添加燃料存储组件
        fuelStorage = new FuelStorageComponent(96);
        AddComponent(fuelStorage);

        // 内容物不允许放入
        innerContents.allowAdd = false;
        innerContents.notAllowAddReason = "该槽位仅用于放置蒸馏产出的瓶装水";

        var states = new List<CardState>()
        {
            new ("未点燃", "22"),
            new ("已点燃", "22", true),
        };
        stateMachine = new StateMachineComponent("未点燃", states);
        AddComponent(stateMachine);

        // 淡水存储组件
        freshWaterStorage = new(12);
        AddComponent(freshWaterStorage);

        // 盐水存储组件
        salineWaterStorage = new(24);
        AddComponent(salineWaterStorage);
    }

    protected override void OnInit()
    {
        fuelStorage.whileBurning = HandleDistillation;

        // 取出瓶装水时，如果淡水储量达到了上限，则再生成一瓶
        innerContents.onRemoveCard = (c) =>
        {
            TryGetBottledWater();
        };
    }

    private void Ignite(CardEvent e)
    {
        PlaySound("点火_02");

        fuelStorage.Ignite();

        // 点燃后暂停所有卡牌每回合更新
        innerContents.FreezeUpdate();

        stateMachine.ChangeState("已点燃");
    }

    private void Extinguish(CardEvent e)
    {
        fuelStorage.Extinguish();

        // 熄灭后恢复所有卡牌每回合更新
        innerContents.UnfreezeUpdate();

        stateMachine.ChangeState("未点燃");
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

        salineWaterStorage.AddValue(-1); // 盐水储量-1
        freshWaterStorage.AddValue(1); // 淡水储量+1

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
        var card = CardFactory.CreateCard("瓶装水");
        AddCard(card, innerContents.bag, false);
        card.RefreshSlot();
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