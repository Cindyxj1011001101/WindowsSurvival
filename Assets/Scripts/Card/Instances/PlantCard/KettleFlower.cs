using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 水壶兰
/// </summary>
public class KettleFlower : Card
{
    private PlantGrowthComponent plant;
    private StateMachineComponent stateMachine;

    public bool hasWound = false;
    public int recoverProgress = 0; // 伤口恢复进度
    public int maxRecoverProgress = 10;

    private KettleFlower()
    {
        Events = new()
        {
            new Event("划一个口", "在水壶兰的茎部划一个口，从而可以饮用其中的汁液，并且有概率获得一颗种子。\n伤口需要一段时间愈合，愈合前水壶兰不会生长", Event_Hurt, Judge_Hurt, () => 15),
            new Event("铲起", "将水壶兰连根铲起。将会获得一颗种子", Event_DigUp, Judge_DigUp, () => 15),
            new Event("饮用汁液", "", Event_Drink, Judge_Drink, () => 15, () => new(){ { PlayerStateEnum.Thirst, +14 }, { PlayerStateEnum.San, -3 } }),
        };
    }

    public override void Awake()
    {
        base.Awake();

        TryGetComponent(out plant);
        plant.onDead = () => AddCard("水壶兰种子", true); // 死亡时获得一颗种子

        if (!TryGetComponent(out stateMachine))
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

    private void Event_Hurt(out string tip)
    {
        Hurt(GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Cut), out tip);
    }

    private void Hurt(Card tool, out string tip)
    {
        tip = string.Empty;
        tool.Use(); // 工具耐久减少

        hasWound = true; // 产生伤口
        plant.AddGrowth(-10); // 生长进度-10
        DisplayComponentValueChange(typeof(PlantGrowthComponent), -10);

        plant.growStopped = true; // 停止生长

        TimeManager.Instance.AddTime(15);

        if (Random.Range(0, 100) <= 5) // 5%概率获得水壶兰种子
        {
            AddCard("水壶兰种子", true);
        }

        UpdatePlantState();
    }

    private bool Judge_Hurt(out string hint)
    {
        hint = string.Empty;
        if (hasWound)
        {
            hint = "已有伤口";
            return false;
        }
        if (plant.growth < 30)
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
        AddCard("水壶兰种子", true);
    }

    private bool Judge_DigUp(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfToolType(ToolType.Dig) == null)
        {
            hint = "需要切割类工具";
            return false;
        }
        return true;
    }

    private void Event_Drink(out string tip)
    {
        tip = string.Empty;
        // 播放喝水的音效
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("喝_01", true);

        plant.AddGrowth(-20); // 生长进度-20
        DisplayComponentValueChange(typeof(PlantGrowthComponent), -20);

        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Thirst, 14);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, -3);

        TimeManager.Instance.AddTime(15);
    }

    private bool Judge_Drink(out string hint)
    {
        hint = string.Empty;
        if (!hasWound)
        {
            hint = "需要切口";
            return false;
        }
        if (plant.growth < 20)
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
            recoverProgress++;
            if (recoverProgress == maxRecoverProgress)
            {
                hasWound = false;
                recoverProgress = 0;
                plant.growStopped = false;
            }
        }

        UpdatePlantState();
    }

    public override bool CanQuickInteract(Card card)
    {
        if (card.TryGetComponent<ToolComponent>(out var component))
        {
            return component.toolTypes.Contains(ToolType.Cut) || component.toolTypes.Contains(ToolType.Dig);
        }

        return base.CanQuickInteract(card);
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        var card = slot.PeekCard();

        if (card.TryGetComponent<ToolComponent>(out var component))
        {
            if (component.toolTypes.Contains(ToolType.Cut))
            {
                Hurt(card, out tip);
                return;
            }
            if (component.toolTypes.Contains(ToolType.Dig))
            {
                DigUp(card, out tip);
                return;
            }
        }

        base.QuickIneract(slot, count, out tip);
    }
}