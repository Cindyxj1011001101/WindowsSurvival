using System.Collections.Generic;

/// <summary>
/// 四角菱
/// </summary>
public class WaterChestnut : Card
{
    private PlantGrowthComponent plant;
    private StateMachineComponent stateMachine;

    private WaterChestnut()
    {
        Events = new()
        {
            new Event("采集", "采集四角菱结出的菱果", Event_Collect, Judge_Collect, () => 15),
            new Event("铲起", "将四角菱连根铲起。将会获得一颗菱果", Event_DigUp, Judge_DigUp, () => 15),
        };
    }

    public override void Awake()
    {
        base.Awake();

        TryGetComponent(out plant);

        if (!TryGetComponent(out stateMachine))
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
    }

    private void UpdatePlantState()
    {
        var growth = plant.growth;

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
        plant.AddGrowth(-100); // 生长进度-100
        TimeManager.Instance.AddTime(15);
        AddCard("菱果", Bag);
        UpdatePlantState();
    }

    private bool Judge_Collect(out string hint)
    {
        hint = string.Empty;
        if (!plant.IsMature)
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
        TimeManager.Instance.AddTime(15);
        AddCard(plant.deadCardId, Bag);
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
            tip = "铲起";
            return true;
        }

        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        DigUp(slot.PeekCard(), out tip);
    }
}