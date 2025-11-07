using System.Collections.Generic;

/// <summary>
/// 四角菱
/// </summary>
public class WaterChestnut : PlantCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("采集", "采集四角菱结出的菱果", Event_Collect, Judge_Collect, () => 15);
        AddCardEvent("铲起", "将四角菱连根铲起。将会获得一颗菱果", Event_DigUp, Judge_DigUp, () => 15);
    }

    protected override void OnLateConstructor()
    {
        var states = new List<CardState>()
        {
            new ("幼苗期", "6"),
            new ("生长期1", "7") { displayName = "生长期"},
            new ("生长期2", "8") { displayName = "生长期"},
            new ("成熟期", "9"),
        };
        stateMachine = new StateMachineComponent(states);
        AddComponent(stateMachine);

        UpdatePlantState();
    }

    protected override void UpdatePlantState()
    {
        var growth = plantGrowth.value;

        // 幼苗期
        if (growth >= 0 && growth <= 10)
        {
            stateMachine.ChangeState("幼苗期");
        }
        else if (growth <= 50)
        {
            stateMachine.ChangeState("生长期1");
        }
        else if (growth < 100)
        {
            stateMachine.ChangeState("生长期2");
        }
        else
        {
            stateMachine.ChangeState("成熟期");
        }
    }

    private void Event_Collect(out string tip)
    {
        tip = string.Empty;
        plantGrowth.AddValue(-100); // 生长进度-100
        ApplyEventEffects(0);
        AddCard("菱果", Bag);
        UpdatePlantState();
    }

    private bool Judge_Collect(out string hint)
    {
        hint = string.Empty;
        if (!IsRipe)
        {
            hint = "四角菱尚未成熟，无法采集";
            return false;
        }
        return true;
    }

    private void Event_DigUp(out string tip)
    {
        DigUp(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig), out tip);
    }

    private void DigUp(Card tool, out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        tool.Use();
        ApplyEventEffects(1);
        AddCard(plantGrowth.deadCardId, Bag);
    }

    private bool Judge_DigUp(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig) == null)
        {
            hint = "需要挖掘类工具";
            return false;
        }
        return true;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        UpdatePlantState();
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        if (card.TryGetComponent<ToolComponent>(out var component) && component.toolTypes.Contains(ToolType.Dig))
        {
            tip = Events[1].Name;
            return true;
        }

        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        DigUp(slot.PeekCard(), out tip);
    }
}