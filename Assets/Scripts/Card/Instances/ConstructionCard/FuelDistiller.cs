using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// 燃料蒸馏器
/// </summary>
public class FuelDistiller : ConstructionCard
{
    private InnerContentsComponent innerContents; // 内容物组件
    private FuelStorageComponent fuelStorage; // 燃料存储组件
    private StateMachineComponent stateMachine;

    public int maxSalineWaterStorage = 24;
    public int maxFreshWaterStorage = 12;
    public int salineWaterStorage = 0;
    public int freshWaterStorage = 0;
    public int fuelConsume = 1;

    private FuelDistiller()
    {
        Events = new()
        {
            new Event("点燃", "点燃蒸馏器，将盐水蒸馏成淡水。点燃状态下会导致室内氧气消耗与一氧化碳增加", Event_Light, Judge_Light),
            new Event("熄灭", "", Event_UnLight, Judge_UnLight),
            new Event("倒入盐水", "消耗盐水，使蒸馏器的盐水储量+12\n！可能会造成浪费！", Event_AddSalineWater, Judge_AddSalineWater),
        };
    }

    public override void LateInit()
    {
        base.LateInit();
        // 手动添加燃料存储组件
        if (!TryGetComponent(out fuelStorage))
        {
            fuelStorage = new FuelStorageComponent(96);
            AddComponent(fuelStorage);
        }

        innerContents.allowAdd = false; // 不允许放入

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
    }

    /// <summary>
    /// 点燃
    /// </summary>
    /// <param name="tip"></param>
    private void Event_Light(out string tip)
    {
        tip = string.Empty;

        var env = Bag as EnvironmentBag;
        env.ChangeEnvironmentStateChangeRate(EnvironmentStateEnum.Oxygen, -4); // 点燃后地点氧气每回合-4
        env.ChangeEnvironmentStateChangeRate(EnvironmentStateEnum.CarbonMonoxideLevel, +2); // 点燃后地点一氧化碳每回合+2

        // 点燃后暂停所有卡牌每回合更新
        innerContents.PauseUpdating();

        fuelStorage.SetIsFiring(true);

        stateMachine.ChangeState("已点燃");
        
        SoundManager.Instance.PlaySound("点火_02");
    }

    private bool Judge_Light(out string hint)
    {
        hint = string.Empty;

        if (StateManager.Instance.WaterLevel.CurValue >= 30)
        {
            hint = "水位过高，无法点燃燃料蒸馏器";
            return false;
        }

        if (fuelStorage.fuel < fuelConsume)
        {
            hint = "燃料不足，无法点燃燃料蒸馏器";
            return false;
        }

        return !fuelStorage.isFiring;
    }

    /// <summary>
    /// 熄灭
    /// </summary>
    /// <param name="tip"></param>
    private void Event_UnLight(out string tip)
    {
        tip = string.Empty;

        var env = Bag as EnvironmentBag;
        env.ChangeEnvironmentStateChangeRate(EnvironmentStateEnum.Oxygen, +4);
        env.ChangeEnvironmentStateChangeRate(EnvironmentStateEnum.CarbonMonoxideLevel, -2);

        // 熄灭后恢复所有卡牌每回合更新
        innerContents.ContinueUpdating();

        fuelStorage.SetIsFiring(false);

        stateMachine.ChangeState("未点燃");
    }

    private bool Judge_UnLight(out string hint)
    {
        hint = string.Empty;
        return fuelStorage.isFiring;
    }

    /// <summary>
    /// 倒入盐水
    /// </summary>
    /// <param name="tip"></param>
    private void Event_AddSalineWater(out string tip)
    {
        tip = string.Empty;
        GameManager.Instance.PlayerBag.FindCardOfName("盐水").DestroyThis();
        salineWaterStorage += 12; // 盐水储量+12
        salineWaterStorage = Mathf.Clamp(salineWaterStorage, 0, maxSalineWaterStorage);
    }

    private bool Judge_AddSalineWater(out string hint)
    {
        hint = string.Empty;

        if (salineWaterStorage >= maxFreshWaterStorage)
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

    protected override void OnUpdate()
    {
        base.OnUpdate();

        // 没有点燃
        if (!fuelStorage.isFiring) return;

        var waterLevel = StateManager.Instance.WaterLevel.CurValue;

        // 这里剩余燃料一定是>=fuelConsume的，因为燃料<fuelConsume时会自动熄灭并且无法点燃
        fuelStorage.AddFuel(-fuelConsume); // 每回合消耗1点燃料
        if (waterLevel > 0) // 水平面>0时，燃料额外-4
        {
            fuelStorage.AddFuel(-4);
        }

        // 处理蒸馏逻辑
        HandleDistillation();

        if (fuelStorage.isFiring && fuelStorage.fuel < fuelConsume) // 燃料不足时自动熄灭
        {
            Event_UnLight(out _);
            ShowTip("燃料不足，燃料蒸馏器已自动熄灭");
            return;
        }

        // 水平面高于30，自动熄灭
        if (fuelStorage.isFiring && waterLevel >= 30)
        {
            fuelStorage.SetIsFiring(false);
            ShowTip("水位过高，燃料蒸馏器已自动熄灭");
        }
    }

    private void HandleDistillation()
    {
        if (salineWaterStorage < 1 || freshWaterStorage >= maxFreshWaterStorage) return;

        salineWaterStorage -= 1; // 盐水储量-1
        freshWaterStorage += 1; // 淡水储量+1

        GetBottledWater();
    }

    private void GetBottledWater()
    {
        // 淡水储量没有达到上限，或者内容物已满，不生成瓶装水
        if (freshWaterStorage < maxFreshWaterStorage || !innerContents.bag.CanAddCard(CardFactory.GetStaticCardInstance("瓶装水"), out _)) return;

        // 淡水储量清0，生成一瓶瓶装水
        freshWaterStorage = 0;
        AddCard("瓶装水", innerContents.bag, out var card);
        card.RefreshSlot();
        ShowTip("蒸馏得到了一瓶瓶装水");
    }

    public override bool CanQuickInteract(Card card)
    {
        // 添加燃料
        if (card.TryGetComponent<FlammableComponent>(out _) && fuelStorage.fuel < fuelStorage.maxFuel) return true;

        // 放入盐水
        if (card.CardId == "盐水" && salineWaterStorage < maxFreshWaterStorage) return true;

        // 拆毁
        return base.CanQuickInteract(card);
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        tip = string.Empty;
        var card = slot.PeekCard();

        // 添加燃料
        if (card.TryGetComponent<FlammableComponent>(out var burnableComponent) && fuelStorage.fuel < fuelStorage.maxFuel)
        {
            card.DestroyThis();
            fuelStorage.AddFuel(burnableComponent.fuelValue);
            return;
        }

        // 放入盐水
        if (card.CardId == "盐水" && salineWaterStorage < maxFreshWaterStorage)
        {
            card.DestroyThis();
            salineWaterStorage += 12;
            salineWaterStorage = Mathf.Clamp(salineWaterStorage, 0, maxSalineWaterStorage);
            return;
        }

        // 拆毁
        base.QuickIneract(slot, count, out tip);
    }
}