using System.Collections.Generic;

/// <summary>
/// 燃料发电机
/// </summary>
public class FuelGenerator : ConstructionCard
{
    private const float POWER_PRODUCTION_RATE = 0.8f;

    protected override void RegisterCardEvents()
    {
        AddCardEvent("点燃", $"点燃{CardName}。点然后每15分钟产生{POWER_PRODUCTION_RATE}单位电力。\n点燃状态下会导致室内氧气加速消耗与一氧化碳增加", fuelStorage.Ignite, CanIgnite);
        AddCardEvent("熄灭", "", fuelStorage.Extinguish, CanExtinguish);
        base.RegisterCardEvents(); // 拆毁
    }

    protected override void OnLateConstructor()
    {
        // 手动添加燃料存储组件
        fuelStorage = new FuelStorageComponent(144);
        AddComponent(fuelStorage);

        var states = new List<CardState>()
        {
            new ("未点燃", "26"),
            new ("已点燃", "26", true, true),
        };
        stateMachine = new StateMachineComponent("未点燃", states);
        AddComponent(stateMachine);

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
        PlaySound("点火_02");

        powerConsumption.ConnectPower();

        stateMachine.ChangeState("已点燃");
    }

    private void OnExtinguish()
    {
        // 只有玩家在同一地点时才停止音效
        if (GameManager.Instance.IsCurrentEnvironment(Bag))
            SoundManager.Instance.StopCardLoopSound(CardId);

        powerConsumption.DisconnectPower();

        stateMachine.ChangeState("未点燃");
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