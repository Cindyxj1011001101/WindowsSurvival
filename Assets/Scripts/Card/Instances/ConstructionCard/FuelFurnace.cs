using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 燃料炉
/// </summary>
public class FuelFurnace : ConstructionCard
{
    private InnerContentsComponent innerContents;
    private FuelContainerComponent fuelContainer;

    public bool isLightened; // 是否已打开
    public List<Card> cardsToProcesss; // 待加工卡牌
    public int leftRounds = 0; // 当前加工轮数
    public int maxRound = 16; // 总加工轮数
    public bool isProcessing = false; // 是否正在加工
    public float curTempture = 0; // 当前温度
    public List<TempertureData> tempertureData = new(); // 温度数据，用来处理产物获取
    public string outComeCardId = null; // 产物卡牌id

    private bool considerWaterLevel => (Bag is EnvironmentBag env) && env.PlaceData.isInSpacecraft; // 是否考虑水平面，当不在飞船内时不考虑水平面

    private FuelFurnace()
    {
        Events = new()
        {
            new Event("点燃", "", Event_Lighting, Judge_Lighting),
            new Event("开始加工" , "", Event_Process, Judge_Process),
            new Event("熄灭" , "", Event_Unlightened, Judge_Unlightened),
            new Event("取出" , "取出燃料炉的加工产物", Event_TakeOut, Judge_TakeOut),
        };
    }

    protected override void LateInit()
    {
        base.LateInit();
        // 添加燃料存储组件
        if (!TryGetComponent(out fuelContainer))
        {
            fuelContainer = new FuelContainerComponent(96);
            AddComponent(fuelContainer);
        }
    }

    private bool ContentFilter(Card c, out string s)
    {
        //YONG-TODO：对过滤器做初始化，限制放入物体的可加工属性
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// 点燃，需要燃料点火器
    /// </summary>
    /// <param name="tip"></param>
    private void Event_Lighting(out string tip)
    {
        LightFuelFurnace(GameManager.Instance.PlayerBag.FindCardOfName("燃料点火器"), out tip);
    }

    /// <summary>
    /// 点燃燃料炉
    /// </summary>
    /// <param name="card"></param>
    /// <param name="tip"></param>
    private void LightFuelFurnace(Card card, out string tip)
    {
        tip = string.Empty;
        card.Use();
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

        if (fuelContainer.fuel < 1)
        {
            hint = "燃料不足，无法点燃燃料炉";
            return false;
        }

        return !isLightened;
    }

    /// <summary>
    /// 取出加工产物
    /// </summary>
    /// <param name="tip"></param>
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

    /// <summary>
    /// 开始加工
    /// </summary>
    /// <param name="tip"></param>
    private void Event_Process(out string tip)
    {
        tip = string.Empty;

        isProcessing = true;
        leftRounds = maxRound;

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

        if (isProcessing)
        {
            hint = "正在加工中";
            return false;
        }

        if (!string.IsNullOrEmpty(outComeCardId))
        {
            hint = "请先取出加工产物";
            return false;
        }

        if (!innerContents.bag.IsFull)
        {
            hint = "燃料炉内必须放满待加工物才能加工";
            return false;
        }

        return true;
    }

    /// <summary>
    /// 熄灭
    /// </summary>
    /// <param name="tip"></param>
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
        HandleFuelAndTemperatureChange();

        HandleProcessRound();
    };

    private void HandleFuelAndTemperatureChange()
    {
        var waterLevel = StateManager.Instance.WaterLevel.CurValue;
        // 点燃状态下
        if (isLightened && fuelContainer.fuel >= 1)
        {
            curTempture += 17; // 温度+17
            fuelContainer.AddFuel(-1); // 燃料-1
            if (considerWaterLevel && waterLevel > 0) // 水平面>0时，燃料额外-4
            {
                fuelContainer.AddFuel(-4);
            }
        }
        // 非点燃状态下
        else
        {
            curTempture -= 4; // 温度-4
            if (considerWaterLevel && waterLevel >= 30) // 水平面>=30时，温度额外-8
            {
                curTempture -= 8;
            }
            else if (considerWaterLevel && waterLevel > 0) // 水平面>=0时，温度额外-4
            {
                curTempture -= 4;
            }
        }

        curTempture = Mathf.Clamp(curTempture, 0, 300); // 温度限制在0~300之间

        if (!isLightened) return;

        // 燃料不足时自动熄灭
        if (fuelContainer.fuel < 1)
        {
            isLightened = false;
            RefreshSlot();
            ShowTip("燃料不足，燃料炉已自动熄灭");
            return;
        }

        // 水平面高于30，自动熄灭
        if (considerWaterLevel && waterLevel >= 30)
        {
            isLightened = false;
            RefreshSlot();
            ShowTip("水位过高，燃料炉已自动熄灭");
        }
    }

    private void HandleProcessRound()
    {
        if (leftRounds <= 0) return;

        if (curTempture <= 50) tempertureData[0].round++;
        else if (curTempture <= 100) tempertureData[1].round++;
        else if (curTempture <= 200) tempertureData[2].round++;
        else tempertureData[3].round++;
        leftRounds--;

        // 加工完成
        if (leftRounds <= 0)
        {
            leftRounds = 0;
            outComeCardId = ProcessManager.GetProcessOutcomeID(cardsToProcesss, tempertureData);
            isProcessing = false;
            tempertureData.Clear();
            innerContents.Clear(); // 销毁内容物
            cardsToProcesss.Clear(); // 销毁加工列表

            //TODO:恢复InnerBag可放入拖出卡牌


            ShowTip("燃料炉加工完成");
        }
    }

    public override bool CanQuickInteract(Card card)
    {
        // 点火
        if (card.CardId == "燃料点火器" &&
            !isLightened &&
            fuelContainer.fuel >= 1 &&
            (!considerWaterLevel || StateManager.Instance.WaterLevel.CurValue < 30))
            return true;
        // 添加燃料
        if (card.TryGetComponent<FlammableComponent>(out _) && fuelContainer.fuel < fuelContainer.maxFuel) return true;
        // 放入内容物
        if (innerContents.CanQuickInteract(card)) return true;
        // 拆毁
        return base.CanQuickInteract(card);
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        tip = string.Empty;
        var card = slot.PeekCard();

        // 点火
        if (card.CardId == "燃料点火器" &&
            !isLightened &&
            fuelContainer.fuel >= 1 &&
            (!considerWaterLevel || StateManager.Instance.WaterLevel.CurValue < 30))
        {
            LightFuelFurnace(card, out tip);
            RefreshSlot();
            return;
        }

        // 添加燃料
        if (card.TryGetComponent<FlammableComponent>(out var burnableComponent) && fuelContainer.fuel < fuelContainer.maxFuel)
        {
            card.DestroyThis();
            fuelContainer.AddFuel(burnableComponent.fuelValue);
            return;
        }

        // 放入内容物
        if (innerContents.CanQuickInteract(card))
        {
            innerContents.QuickIneract(slot, count, out tip);
            return;
        }

        // 拆毁
        base.QuickIneract(slot, count, out tip);
    }
}