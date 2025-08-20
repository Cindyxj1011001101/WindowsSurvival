using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 燃料炉
/// </summary>
public class FuelFurnace : ConstructionCard
{
    private InnerContentsComponent innerContents;
    private FuelComponent fuelComponent;

    public bool isLightened; // 是否已打开
    public List<Card> cardsToProcesss;
    public int curRounds = 0;
    public int maxRound = 16;
    public bool isProcessing = false;
    public float curTempture = 0;
    public List<TempertureData> tempertureData = new();
    public string outComeCardId = null;

    //TODO:将拥有BurnableComponent卡牌拖拽到本卡牌上，增加燃料

    private bool considerWaterLevel => (Bag is EnvironmentBag env) && env.PlaceData.isInSpacecraft;

    private FuelFurnace()
    {
        Events = new()
        {
            new Event("点燃", "", Event_Lighting, Judge_Lighting),
            new Event("开始加工" , "", Event_Process, Judge_Process),
            new Event("熄灭" , "", Event_Unlightened, Judge_Unlightened),
            new Event("取出" , "", Event_TakeOut, Judge_TakeOut),
        };
    }

    protected override void LateInit()
    {
        base.LateInit();
        if (!TryGetComponent(out fuelComponent))
        {
            fuelComponent = new FuelComponent(96);
            AddComponent(fuelComponent);
        }
        // 监听水平面变化，水平面过高时熄灭燃料炉
        EventManager.Instance.AddListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnWaterLevelChanged);
    }

    private void OnWaterLevelChanged(RefreshEnvironmentStateArgs args)
    {
        // 不在飞船内时不处理，因为没有水平面属性
        if (!considerWaterLevel) return;

        if (args.stateEnum != EnvironmentStateEnum.WaterLevel) return;

        if (args.stateValue.CurValue >= 30 && isLightened)
        {
            isLightened = false;
            EventManager.Instance.TriggerEvent(EventType.ChangeCardProperty, this);
            ShowTip("水位过高，燃料炉已自动熄灭");
        }
    }

    public override void DestroyThis()
    {
        base.DestroyThis();
        EventManager.Instance.RemoveListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnWaterLevelChanged);
    }

    private bool ContentFilter(Card c, out string s)
    {
        //YONG-TODO：对过滤器做初始化，限制放入物体的可加工属性
        throw new System.NotImplementedException();
    }

    private void Event_Lighting(out string tip)
    {
        tip = string.Empty;
        GameManager.Instance.PlayerBag.FindCardOfName("燃料点火器").Use();
        isLightened = true;
    }

    private bool Judge_Lighting(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfName("燃料点火器") == null)
        {
            hint = "需要燃料点火器";
            return false;
        }

        if (considerWaterLevel && StateManager.Instance.WaterLevel.CurValue >= 30)
        {
            hint = "水位过高，无法点燃燃料炉";
            return false;
        }

        if (fuelComponent.fuel < 1)
        {
            hint = "燃料不足，无法点燃燃料炉";
            return false;
        }

        return !isLightened;
    }

    private void Event_TakeOut(out string tip)
    {
        tip = string.Empty;
        AddCard(outComeCardId, true);
        outComeCardId = null;
    }

    private bool Judge_TakeOut(out string hint)
    {
        hint = string.Empty;
        return !string.IsNullOrEmpty(outComeCardId);
    }

    private void Event_Process(out string tip)
    {
        tip = string.Empty;

        isProcessing = true;
        curRounds = maxRound;

        // TODO: 不可以添加或移除内容物


        // 记录当前炉内的卡牌，即需要加工的卡牌
        foreach (var slot in innerContents.bag.Slots)
        {
            foreach (var card in slot.Cards)
            {
                card.StopUpdating(); // 停止更新卡牌状态
                cardsToProcesss.Add(card);
            }
        }

        // 记录温度状态
        tempertureData = new()
        {
            new (TempertureType.Normal, 0),
            new (TempertureType.Low, 0),
            new (TempertureType.Medium, 0),
            new (TempertureType.High, 0)
        };
    }

    private bool Judge_Process(out string hint)
    {
        hint = string.Empty;

        if (isProcessing) return false;

        if (!string.IsNullOrEmpty(outComeCardId))
        {
            hint = "请先取出加工产物";
            return false;
        }

        if (!innerContents.bag.IsFull)
        {
            hint = "燃料炉内必须放满代加工物才能加工";
            return false;
        }

        return true;
    }

    private void Event_Unlightened(out string tip)
    {
        tip = string.Empty;
        isLightened = false;
    }

    private bool Judge_Unlightened(out string hint)
    {
        hint = string.Empty;
        return !isLightened;
    }

    protected override System.Action OnUpdate => () =>
    {
        var waterLevel = StateManager.Instance.WaterLevel.CurValue;
        if (isLightened && fuelComponent.fuel >= 1)
        {
            curTempture += 17; // 温度+17
            fuelComponent.AddFuel(-1); // 燃料-1
            if (considerWaterLevel && waterLevel > 0) // 水平面>0时，燃料额外-4
            {
                fuelComponent.AddFuel(-4);
            }

            if (fuelComponent.fuel < 1 && isLightened) // 燃料不足时自动熄灭
            {
                isLightened = false;
                EventManager.Instance.TriggerEvent(EventType.ChangeCardProperty, this);
                ShowTip("燃料不足，燃料炉已自动熄灭");
                return;
            }
        }
        else
        {
            curTempture -= 4;
            if (considerWaterLevel && waterLevel > 0)
            {
                curTempture -= 4;
            }
            if (considerWaterLevel && waterLevel >= 30)
            {
                curTempture -= 8;
            }
        }

        curTempture = Mathf.Clamp(curTempture, 0, 300);


        if (curRounds != 0)
        {
            if (curTempture <= 50)
            {
                tempertureData[0].Round++;
            }
            else if (curTempture <= 100)
            {
                tempertureData[1].Round++;
            }
            else if (curTempture <= 200)
            {
                tempertureData[2].Round++;
            }
            else
            {
                tempertureData[3].Round++;
            }
            curRounds--;
        }
        else
        {
            string outcomeID = ProcessManager.GetProcessOutcomeID(cardsToProcesss, tempertureData);
            isProcessing = false;
            outComeCardId = outcomeID;
            innerContents.Clear(); // 销毁内容物
            cardsToProcesss.Clear(); // 销毁加工列表
            //TODO:恢复InnerBag可放入拖出卡牌
        }
    };

    public override bool CanQuickInteract(Card card)
    {
        // 添加燃料
        if (card.TryGetComponent<FlammableComponent>(out _) && fuelComponent.fuel < fuelComponent.maxFuel)
        {
            return true;
        }
        // 放入内容物
        if (innerContents.CanQuickInteract(card)) return true;
        // 拆毁
        return base.CanQuickInteract(card);
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        tip = string.Empty;
        var card = slot.PeekCard();

        // 添加燃料
        if (card.TryGetComponent<FlammableComponent>(out var burnableComponent) && fuelComponent.fuel < fuelComponent.maxFuel)
        {
            card.DestroyThis();
            fuelComponent.AddFuel(burnableComponent.fuelValue);
            return;
        }

        // 放入内容物
        if (innerContents.CanQuickInteract(card))
        {
            innerContents.QuickIneract(slot, count, out tip);
            return;
        }

        base.QuickIneract(slot, count, out tip);
    }
}