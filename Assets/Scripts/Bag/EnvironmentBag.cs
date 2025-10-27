using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class EnvironmentBag : Bag
{
    [JsonProperty] private PlaceEnum placeType;
    [JsonProperty] private bool hasCable;
    [JsonProperty] private PressureLevel pressureLevel;
    [JsonProperty] private DropList disposableDropList = new();
    [JsonProperty] private DeepExploreDropList repeatableDropList = new();
    [JsonProperty] private Dictionary<EnvironmentStateEnum, State> stateDict = new();

    [JsonIgnore] public bool HasCable => hasCable;
    [JsonIgnore] public PressureLevel PressureLevel => pressureLevel;
    [JsonIgnore] public string PlaceName => GameManager.Instance.PlaceDataDict[placeType].placeName;
    [JsonIgnore] public DropList DisposableDropList => disposableDropList;
    [JsonIgnore] public DeepExploreDropList RepeatableDropList => repeatableDropList;
    [JsonIgnore] public Dictionary<EnvironmentStateEnum, State> StateDict => stateDict;
    [JsonIgnore] public PlaceData PlaceData => GameManager.Instance.PlaceDataDict[placeType];
    [JsonIgnore] public float DiscoveryDegree => 1 - DisposableDropList.RemainingDropsRate;
    [JsonIgnore] public bool ExploreCompleted => DisposableDropList.IsEmpty && RepeatableDropList.IsEmpty;
    [JsonIgnore] public List<IEntity> Entities { get; private set; } = new();

    public void SetPlaceType(PlaceEnum placeType)
    {
        this.placeType = placeType;
    }

    #region Init
    protected override void FirstInit()
    {
        AddSlot(9);

        FirstInitState();
        FirstInitDropList();

        hasCable = PlaceData.initialBagStateConfig.hasCable;
        foreach (var cardId in PlaceData.initialBagStateConfig.containedCards)
        {
            AddCard(CardFactory.CreateCard(cardId));
        }

        pressureLevel = PlaceData.initialBagStateConfig.pressureLevel;
    }

    public override void Init()
    {
        base.Init();
        InitEntitesAndCardLocation();
        RepeatableDropList.StartUpdating();
        EventManager.Instance.AddListener(EventType.UpdateBegin, OnEnvUpdateBegin);
        // 每回合结算地点状态
        UpdateManager.Instance.EnvironmentUpdate.AddListener(EnvUpdate);
    }

    private void FirstInitState()
    {
        // 在室内且非水域显示氧气
        // 在室内且非水域显示一氧化碳
        if (PlaceData.isIndoor && !PlaceData.isInWater)
        {
            StateDict.Add(EnvironmentStateEnum.Oxygen, new State(UnityEngine.Random.Range(400, 600), 1000));
            StateDict.Add(EnvironmentStateEnum.COLevel, new State(0, 100, -0.5f));
        }

        // 室温
        stateDict.Add(EnvironmentStateEnum.RoomTemperature, new State(200, 400, normParam: -200));
    }

    private void FirstInitDropList()
    {
        disposableDropList = JsonManager.DeepCopy(CardFactory.GetDisposableDropList(PlaceData.placeType));
        repeatableDropList = JsonManager.DeepCopy(CardFactory.GetDeepExploreDropList(PlaceData.placeType));
    }

    /// <summary>
    /// 将地点内的所有实体加入实体列表
    /// </summary>
    private void InitEntitesAndCardLocation()
    {
        foreach (var slot in Slots)
        {
            foreach (var card in slot.Cards)
            {
                if (card is IEntity entity)
                {
                    AddEntity(entity);
                }
                else if (card.TryGetComponent<CoordinateComponent>(out var c))
                {
                    c.coordinate.SetLocation(this);
                }
            }
        }

        if (GameManager.Instance.IsCurrentEnvironment(this))
        {
            AddEntity(GameManager.Instance.Player);
        }
    }
    #endregion

    #region Update
    private Dictionary<EnvironmentStateEnum, float> envStateChangeRatesSnapshot = new(); // 记录地点状态的当前变化率，防止地点状态的结算顺序影响结算结果

    private void OnEnvUpdateBegin()
    {
        // 记录所有状态变化率的快照
        envStateChangeRatesSnapshot.Clear();
        foreach (var (type, state) in stateDict)
        {
            if (state.ChangeRate != 0)
            {
                envStateChangeRatesSnapshot.Add(type, state.ChangeRate);
            }
        }
    }

    private void EnvUpdate()
    {
        ApplyEnvEffects(envStateChangeRatesSnapshot);
    }
    #endregion

    /// <summary>
    /// 改变环境状态，电力变化不要在这里处理
    /// </summary>
    /// <param name="stateEnum"></param>
    /// <param name="delta"></param>
    public void ChangeEnvironmentState(EnvironmentStateEnum stateEnum, float delta)
    {
        switch (stateEnum)
        {
            case EnvironmentStateEnum.Electricity:
                StateManager.Instance.ChangeElectricity(delta);
                break;
            case EnvironmentStateEnum.WaterLevel:
                StateManager.Instance.ChangeWaterLevel(delta);
                break;
            case EnvironmentStateEnum.HasCable:
            case EnvironmentStateEnum.PressureLevel:
                throw new ArgumentException("修改是否铺设电缆或压强请通过ChangeHasCable/ChangePressureLevel方法");
            default:
                // 没有这个状态不处理
                if (!StateDict.ContainsKey(stateEnum)) return;
                var state = StateDict[stateEnum];
                state.AddValue(delta);
                // 刷新前端显示
                EventManager.Instance.TriggerEvent(EventType.RefreshEnvironmentState, new RefreshEnvironmentStateArgs(PlaceData.placeType, stateEnum)
                {
                    stateValue = state
                });
                break;
        }
    }

    /// <summary>
    /// 改变环境状态的变化率，电力变化不要在这里处理
    /// </summary>
    /// <param name="stateEnum"></param>
    /// <param name="delta"></param>
    public void ChangeEnvironmentStateChangeRate(EnvironmentStateEnum stateEnum, float delta)
    {
        switch (stateEnum)
        {
            case EnvironmentStateEnum.Electricity:
            case EnvironmentStateEnum.WaterLevel:
                throw new ArgumentException("修改电力或水平面请通过StateManager.Instance.ChangeElectricityChangeRate/ChangeWaterLevelChangeRate方法");
            case EnvironmentStateEnum.HasCable:
            case EnvironmentStateEnum.PressureLevel:
                throw new ArgumentException("修改是否铺设电缆或压强请通过ChangeHasCableChangeRate/ChangePressureLevelChangeRate方法");
            default:
                // 没有这个状态不处理
                if (!StateDict.ContainsKey(stateEnum)) return;
                var state = StateDict[stateEnum];
                state.AddChangeRate(delta);
                // 刷新前端显示
                EventManager.Instance.TriggerEvent(EventType.RefreshEnvironmentState, new RefreshEnvironmentStateArgs(PlaceData.placeType, stateEnum)
                {
                    stateValue = state
                });
                break;
        }
    }

    public void ApplyEnvEffects(Dictionary<EnvironmentStateEnum, float> envEffects)
    {
        if (envEffects.IsNullOrEmpty()) return;

        foreach (var (state, delta) in envEffects)
        {
            ChangeEnvironmentState(state, delta);
        }
    }

    // TODO
    //private void OnWaterLevelChanged(float level)
    //{
    //    // 如果当前是水域环境
    //    if (placeData.isInWater)
    //    {
    //        // 如果水平面下降
    //        if (level < StateManager.Instance.WaterLevel.MaxValue)
    //            // 变回陆地环境
    //            placeData.isInWater = false;
    //    }
    //    // 如果当前是陆地环境
    //    else
    //    {
    //        if (level >= StateManager.Instance.WaterLevel.MaxValue)
    //            // 变成水域环境
    //            placeData.isInWater = true;
    //    }
    //}

    public override bool CanAddCard(Card card, out string tip)
    {
        tip = string.Empty;
        return true;
    }

    public override void AddCard(Card card)
    {
        // 如果放不下，就新增格子
        if (!base.CanAddCard(card, out _))
        {
            // 暂定每次新增3个格子
            AddSlot(3);
        }

        base.AddCard(card);

        // 如果剩余格子数量小于3个
        if (EmptySlotCount < 3)
        {
            // 暂定每次新增3个格子
            AddSlot(3);
        }
    }

    public override bool CompactCards()
    {
        var hasChanged = base.CompactCards();
        while (Slots.Count - 3 >= 9 && EmptySlotCount - 3 >= 3)
        {
            RemoveSlot(Slots[^1]);
            RemoveSlot(Slots[^1]);
            RemoveSlot(Slots[^1]);
        }
        //if (Window != null) Window.RefreshDisplay();
        return hasChanged;
    }

    public void AddEntity(IEntity entity)
    {
        if (Entities.Contains(entity)) return;

        // 设置当前所在地点
        entity.Coordinate.SetLocation(this);
        // 将实体加入实体列表
        Entities.Add(entity);
    }

    public void RemoveEntity(IEntity entity)
    {
        entity.Coordinate.SetLocation(null);
        Entities.Remove(entity);
    }

    public override void OnAddCard(Card card)
    {
        base.OnAddCard(card);

        if (card is IEntity entity)
        {
            AddEntity(entity);
        }
        else if (card.TryGetComponent<CoordinateComponent>(out var c))
        {
            c.coordinate.SetLocation(this);
        }
    }

    public override void OnRemoveCard(Card card)
    {
        base.OnRemoveCard(card);

        if (card is IEntity entity)
        {
            RemoveEntity(entity);
        }
        else if (card.TryGetComponent<CoordinateComponent>(out var c))
        {
            c.coordinate.SetLocation(null);
        }
    }
}
