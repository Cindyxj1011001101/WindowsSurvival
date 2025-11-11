using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 水壶兰
/// </summary>
public class KettleFlower : PlantCard
{
    [JsonProperty] private bool hasWound = false;
    [JsonProperty] private int recovery = 0; // 伤口恢复进度
    private const int MAX_RECOVERY = 10;

    protected override void RegisterCardEvents()
    {
        AddCardEvent("划一个口", "在水壶兰的茎部划一个口，从而可以饮用其中的汁液，并且有概率获得一颗种子。\n伤口需要一段时间愈合，愈合前水壶兰不会生长", Event_Hurt, Judge_Hurt, () => 15);
        AddCardEvent("铲起", "将水壶兰连根铲起。将会获得一颗种子", Event_DigUp, Judge_DigUp, () => 15);
        AddCardEvent("饮用汁液", "", Event_Drink, Judge_Drink,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hydration, +14 },
                { PlayerStateEnum.Sanity, -3 }
            },
            sound: "喝_01");
    }

    protected override void OnLateConstructor()
    {
        var states = new List<CardState>()
        {
            new ("幼苗期", "0"),
            new ("生长期1", "1") { displayName = "生长期"},
            new ("生长期2", "2") { displayName = "生长期"},
            new ("成熟期", "3"),
            new ("有伤口1", "4") { displayName = "有伤口"},
            new ("有伤口2", "5") { displayName = "有伤口" },
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
            if (hasWound) stateMachine.ChangeState("有伤口1");
            else stateMachine.ChangeState("生长期1");
        }
        else if (growth < 100)
        {
            if (hasWound) stateMachine.ChangeState("有伤口2");
            else stateMachine.ChangeState("生长期2");
        }
        else
        {
            stateMachine.ChangeState("成熟期");
        }
    }

    private void Hurt(Card tool, CardEvent e)
    {
        tool.Use(); // 工具耐久减少
        hasWound = true; // 产生伤口
        AddPlantGrowth(-10); // 生长进度-10
        plantGrowth.growStopped = true; // 停止生长

        ApplyEventEffects(e, () =>
        {
            if (Random.value <= 0.05) // 5%概率获得水壶兰种子
            {
                AddCard("水壶兰种子", Bag);
            }
        });
    }

    private void Event_Hurt(out string tip, CardEvent e)
    {
        tip = string.Empty;
        Hurt(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut), e);
    }

    private bool Judge_Hurt(out string hint)
    {
        hint = string.Empty;
        if (hasWound)
        {
            hint = "已有伤口";
            return false;
        }
        if (plantGrowth.value < 30)
        {
            hint = "需要生长度大于等于30%";
            return false;
        }
        if (GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut) == null)
        {
            hint = "需要切割类工具";
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

    private void Event_DigUp(out string tip, CardEvent e)
    {
        tip = string.Empty;
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

    private void Event_Drink(out string tip, CardEvent e)
    {
        tip = string.Empty;
        AddPlantGrowth(-20); // 生长进度-20
        ApplyEventEffects(e);
    }

    private bool Judge_Drink(out string hint)
    {
        hint = string.Empty;
        if (!hasWound)
        {
            hint = "需要切口";
            return false;
        }
        if (plantGrowth.value < 20)
        {
            hint = "需要生长度大于等于20%";
            return false;
        }
        return true;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (hasWound)
        {
            recovery++;
            if (recovery == MAX_RECOVERY)
            {
                hasWound = false;
                recovery = 0;
                plantGrowth.growStopped = false;
            }
        }

        UpdatePlantState();
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        if (card.TryGetComponent<ToolComponent>(out var component))
        {
            if (component.toolTypes.Contains(ToolType.Cut))
            {
                tip = Events[0].Name;
                return true;
            }
            if (component.toolTypes.Contains(ToolType.Dig))
            {
                tip = Events[1].Name;
                return true;
            }
        }

        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        tip = string.Empty;
        var card = slot.PeekCard();

        if (card.TryGetComponent<ToolComponent>(out var component))
        {
            if (component.toolTypes.Contains(ToolType.Cut))
            {
                Hurt(card, Events[0]);
                return;
            }
            if (component.toolTypes.Contains(ToolType.Dig))
            {
                DigUp(card, Events[1]);
                return;
            }
        }
    }
}