/// <summary>
/// 四角菱
/// </summary>
[CardId("四角菱")]
public class WaterChestnut : PlantCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("采集", "采集四角菱结出的菱果", Event_Collect, Judge_Collect, () => 15, sound: "采摘植物或采摘果子的音效");
        AddCardEvent("铲起", "将四角菱连根铲起。将会获得一颗菱果", Event_DigUp, Judge_DigUp, () => 15, sound: "挖掘废料_01");
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

    private void Event_Collect(CardEvent e)
    {
        AddPlantGrowth(-100); // 生长进度-100
        ApplyEventEffects(e, () =>
        {
            AddCard("菱果", Bag);
        });
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

    private void DigUp(Card tool, CardEvent e)
    {
        tool.Use();
        ApplyEventEffects(e, () =>
        {
            DestroyThis();
            AddCard(plantGrowth.deadCardId, Bag);
        });
    }

    private void Event_DigUp(CardEvent e)
    {
        DigUp(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig), e);
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

    public override void QuickIneract(SlotCards slot, int count)
    {
        DigUp(slot.PeekCard(), Events[1]);
    }
}