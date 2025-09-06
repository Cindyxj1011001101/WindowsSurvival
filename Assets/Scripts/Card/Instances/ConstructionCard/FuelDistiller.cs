using System.Collections.Generic;

/// <summary>
/// 燃料蒸馏器
/// </summary>
public class FuelDistiller : ConstructionCard
{
    private InnerContentsComponent innerContents; // 内容物组件
    private FuelStorageComponent fuelStorage; // 燃料存储组件
    private StateMachineComponent stateMachine;
    private FreshWaterStorageComponent freshWaterStorage; // 淡水存储 
    private SalineWaterStorageComponent salineWaterStorage; // 盐水存储

    private FuelDistiller()
    {
        Events = new()
        {
            new Event("倒入盐水", "消耗盐水，使蒸馏器的盐水储量+12\n！可能会造成浪费！", Event_AddSalineWater, Judge_AddSalineWater),
        };
    }

    public override void Awake()
    {
        base.Awake();
        // 手动添加燃料存储组件
        if (!TryGetComponent(out fuelStorage))
        {
            fuelStorage = new FuelStorageComponent(96, 1);
            AddComponent(fuelStorage);
        }

        fuelStorage.actionOnIgnite = () =>
        {
            var env = Bag as EnvironmentBag;
            env.ChangeEnvironmentStateChangeRate(EnvironmentStateEnum.Oxygen, -4); // 点燃后地点氧气每回合-4
            env.ChangeEnvironmentStateChangeRate(EnvironmentStateEnum.CarbonMonoxideLevel, +2); // 点燃后地点一氧化碳每回合+2

            // 点燃后暂停所有卡牌每回合更新
            innerContents.PauseUpdating();

            stateMachine.ChangeState("已点燃");

            SoundManager.Instance.PlaySound("点火_02");
        };

        fuelStorage.actionOnExtinguish = () =>
        {
            var env = Bag as EnvironmentBag;
            env.ChangeEnvironmentStateChangeRate(EnvironmentStateEnum.Oxygen, +4);
            env.ChangeEnvironmentStateChangeRate(EnvironmentStateEnum.CarbonMonoxideLevel, -2);

            // 熄灭后恢复所有卡牌每回合更新
            innerContents.ContinueUpdating();

            stateMachine.ChangeState("未点燃");
        };

        fuelStorage.actionWhileBurning = HandleDistillation;

        // 添加点燃熄灭事件
        fuelStorage.AddEvents("点燃蒸馏器。将盐水蒸馏成淡水。\n点燃状态下会导致室内氧气消耗与一氧化碳增加");

        // 不允许放入
        innerContents.allowAdd = false;
        innerContents.notAllowAddReason = "该槽位仅用于放置蒸馏产出的瓶装水";

        // 取出瓶装水时，如果淡水储量达到了上限，则再生成一瓶
        innerContents.onRemoveCard = (c) =>
        {
            GetBottledWater();
        };

        if (!TryGetComponent(out stateMachine))
        {
            var states = new List<CardState>()
            {
                new ("未点燃", "22"),
                new ("已点燃", "22", true),
            };
            stateMachine = new StateMachineComponent("未点燃", states);
            AddComponent(stateMachine);
        }

        if (!TryGetComponent(out freshWaterStorage))
        {
            freshWaterStorage = new(12);
            AddComponent(freshWaterStorage);
        }

        if (!TryGetComponent(out salineWaterStorage))
        {
            salineWaterStorage = new(24);
            AddComponent(salineWaterStorage);
        }
    }

    /// <summary>
    /// 倒入盐水
    /// </summary>
    /// <param name="tip"></param>
    private void Event_AddSalineWater(out string tip)
    {
        tip = string.Empty;
        GameManager.Instance.PlayerBag.FindCardOfName("盐水").DestroyThis();
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

        GetBottledWater();
    }

    /// <summary>
    /// 获取瓶装水
    /// </summary>
    private void GetBottledWater()
    {
        // 淡水储量没有达到上限，或者内容物已满，不生成瓶装水
        if (freshWaterStorage.value < freshWaterStorage.maxValue || !innerContents.bag.CanAddCard(CardFactory.GetStaticCardInstance("瓶装水"), out _)) return;

        // 淡水储量清0，生成一瓶瓶装水
        freshWaterStorage.SetValue(0);
        AddCard("瓶装水", innerContents.bag, out var card);
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
            tip = "倒入盐水";
            return true;
        }

        // 拆毁
        return base.CanQuickInteract(card, out tip);
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        tip = string.Empty;
        var card = slot.PeekCard();

        // 添加燃料
        if (fuelStorage.CanQuickInteract(card))
        {
            fuelStorage.QuickIneract(slot, count, out tip);
            return;
        }

        // 放入盐水
        if (card.CardId == "盐水" && salineWaterStorage.value < salineWaterStorage.maxValue)
        {
            card.DestroyThis();
            salineWaterStorage.AddValue(12);
            return;
        }

        // 拆毁
        base.QuickIneract(slot, count, out tip);
    }
}