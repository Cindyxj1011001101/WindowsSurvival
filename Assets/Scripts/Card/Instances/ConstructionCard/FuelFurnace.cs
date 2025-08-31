using System.Collections.Generic;

/// <summary>
/// 燃料炉
/// </summary>
public class FuelFurnace : ConstructionCard
{
    public override string ExtraInfo
    {
        get
        {
            if (!string.IsNullOrEmpty(outcomeCardId)) return "加工完成";
            else if (isProcessing) return "加工中";
            else return base.ExtraInfo;
        }
    }

    private InnerContentsComponent innerContents;
    private FuelStorageComponent fuelStorage;
    private TemperatureComponent temperatureComponent;

    public List<Card> cardsToProcesss; // 待加工卡牌
    public int leftRounds = 0; // 当前加工轮数
    public int maxRound = 16; // 总加工轮数
    public bool isProcessing = false; // 是否正在加工
    public List<TempertureData> tempertureData = new(); // 温度数据，用来处理产物获取
    public string outcomeCardId = null; // 产物卡牌id
    public override bool HasLoopSound => true;

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

    public override void LateInit()
    {
        base.LateInit();
        // 添加燃料存储组件
        if (!TryGetComponent(out fuelStorage))
        {
            fuelStorage = new FuelStorageComponent(96);
            AddComponent(fuelStorage);
        }
        // 添加温度组件
        if (!TryGetComponent(out temperatureComponent))
        {
            temperatureComponent = new TemperatureComponent(0, 300);
            AddComponent(temperatureComponent);
        }
        // 每个卡牌槽的最大堆叠数都为1
        foreach (var slot in innerContents.bag.Slots)
        {
            slot.SetMaxStackNum(1);
        }
    }

    private bool ContentFilter(Card c, out string s)
    {
        s = string.Empty;
        if (!c.TryGetComponent<FoodPropertyComponent>(out _))
        {
            s = "只能放入可加工的卡牌";
            return false;
        }

        return true;
    }

    /// <summary>
    /// 点燃
    /// </summary>
    /// <param name="tip"></param>
    private void Event_Lighting(out string tip)
    {
        tip = string.Empty;
        fuelStorage.SetIsFiring(true);
        SoundManager.Instance.PlaySound("点火_02");

        var env = Bag as EnvironmentBag;
        
        if (env == GameManager.Instance.CurEnvironmentBag)
            SoundManager.Instance.PlayCardLoopSound(CardId, "燃料炉音效", 1f);
    }

    private bool Judge_Lighting(out string hint)
    {
        hint = string.Empty;

        if (StateManager.Instance.WaterLevel.CurValue >= 30)
        {
            hint = "水位过高，无法点燃燃料炉";
            return false;
        }

        if (fuelStorage.fuel < 1)
        {
            hint = "燃料不足，无法点燃燃料炉";
            return false;
        }

        return !fuelStorage.isFiring;
       
    }

    /// <summary>
    /// 取出加工产物
    /// </summary>
    /// <param name="tip"></param>
    private void Event_TakeOut(out string tip)
    {
        tip = string.Empty;
        AddCard(outcomeCardId, true);
        outcomeCardId = null;
    }

    private bool Judge_TakeOut(out string hint)
    {
        hint = string.Empty;
        if (string.IsNullOrEmpty(outcomeCardId))
        {
            hint = "没有加工产物可取出";
            return false;
        }
        return true;
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

        // 暂停内容物的更新
        innerContents.PauseUpdating();

        // 不可以添加或移除内容物
        innerContents.allowAdd = innerContents.allowRemove = false;

        // 记录当前炉内的卡牌，即需要加工的卡牌
        foreach (var slot in innerContents.bag.Slots)
        {
            foreach (var card in slot.Cards)
            {
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

        if (!string.IsNullOrEmpty(outcomeCardId))
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
        fuelStorage.SetIsFiring(false);

        var env = Bag as EnvironmentBag;
        if (env == GameManager.Instance.CurEnvironmentBag)
            SoundManager.Instance.StopCardLoopSound(CardId);
    }

    private bool Judge_Unlightened(out string hint)
    {
        hint = string.Empty;
        return fuelStorage.isFiring;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        HandleFuelAndTemperatureChange();

        HandleProcessRound();
    }

    /// <summary>
    /// 处理每回合燃料和温度的变化
    /// </summary>
    private void HandleFuelAndTemperatureChange()
    {
        var waterLevel = StateManager.Instance.WaterLevel.CurValue;
        // 点燃状态下
        if (fuelStorage.isFiring)
        {
            temperatureComponent.AddTemperature(17); // 温度+17
            fuelStorage.AddFuel(-1); // 燃料-1
            if (waterLevel > 0) // 水平面>0时，燃料额外-4
            {
                fuelStorage.AddFuel(-4);
            }
        }
        // 非点燃状态下
        else
        {
            temperatureComponent.AddTemperature(-4); // 温度-4
            if (waterLevel >= 30) // 水平面>=30时，温度额外-8
            {
                temperatureComponent.AddTemperature(-8);
            }
            else if (waterLevel > 0) // 水平面>=0时，温度额外-4
            {
                temperatureComponent.AddTemperature(-4);
            }
        }

        // 燃料不足时自动熄灭
        if (fuelStorage.isFiring && fuelStorage.fuel < 1)
        {
            fuelStorage.SetIsFiring(false);
            ShowTip("燃料不足，燃料炉已自动熄灭");
            return;
        }

        // 水平面高于30，自动熄灭
        if (fuelStorage.isFiring && waterLevel >= 30)
        {
            fuelStorage.SetIsFiring(false);
            ShowTip("水位过高，燃料炉已自动熄灭");
        }
    }

    /// <summary>
    /// 处理每回合加工进度变化
    /// </summary>
    private void HandleProcessRound()
    {
        if (leftRounds <= 0) return;

        if (temperatureComponent.temperature <= 50) tempertureData[0].round++;
        else if (temperatureComponent.temperature <= 100) tempertureData[1].round++;
        else if (temperatureComponent.temperature <= 200) tempertureData[2].round++;
        else tempertureData[3].round++;
        leftRounds--;

        // 加工完成
        if (leftRounds <= 0)
        {
            leftRounds = 0;
            outcomeCardId = ProcessManager.GetProcessOutcomeID(cardsToProcesss, tempertureData);
            isProcessing = false;
            tempertureData.Clear();
            innerContents.Clear(); // 销毁内容物
            cardsToProcesss.Clear(); // 销毁加工列表

            // 可放入拖出卡牌
            innerContents.allowAdd = innerContents.allowRemove = true;
            RefreshSlot();
            ShowTip("燃料炉加工完成");
        }
    }

    public override bool CanQuickInteract(Card card)
    {
        // 添加燃料
        if (fuelStorage.CanQuickInteract(card)) return true;
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
        if (fuelStorage.CanQuickInteract(card))
        {
            fuelStorage.QuickIneract(slot, count, out tip);
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
    public override void OnEnterEnvironment()
    {
        if (fuelStorage.isFiring)
            SoundManager.Instance.PlayCardLoopSound(CardId, "燃料炉音效", 0.3f);
    }
    public override void OnLeaveEnvironment()
    {
        SoundManager.Instance.StopCardLoopSound(CardId);
    }
    public override void OnDetailOpen()
    {
        SoundManager.Instance.SetCardLoopVolume(CardId, 1.0f); // 音量调高
    }
    public override void OnDetailClose()
    {
        SoundManager.Instance.SetCardLoopVolume(CardId, 0.3f); // 恢复正常
    }
    public override void DestroyThis()
    {
        OnLeaveEnvironment();
        base.DestroyThis();
    }
}