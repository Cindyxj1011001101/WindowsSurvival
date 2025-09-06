using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentBag : Bag
{
    [JsonProperty] private string placeName;
    [JsonProperty] private bool hasCable;
    [JsonProperty] private PressureLevel pressureLevel;
    [JsonProperty] private DisposableDropList disposableDropList = new();
    [JsonProperty] private RepeatableDropList repeatableDropList = new();
    [JsonProperty] private Dictionary<EnvironmentStateEnum, State> stateDict = new();

    private PlaceData placeData;

    [JsonIgnore] public bool HasCable => hasCable;
    [JsonIgnore] public PressureLevel PressureLevel => pressureLevel;
    [JsonIgnore] public string PlaceName => placeName;
    [JsonIgnore] public DisposableDropList DisposableDropList => disposableDropList;
    [JsonIgnore] public RepeatableDropList RepeatableDropList => repeatableDropList;
    [JsonIgnore] public Dictionary<EnvironmentStateEnum, State> StateDict => stateDict;

    [JsonIgnore]
    public PlaceData PlaceData
    {
        get
        {
            placeData = placeData != null ? placeData : Resources.Load<PlaceData>("ScriptableObject/Place/" + placeName);
            return placeData;
        }
    }

    [JsonIgnore]
    public float DiscoveryDegree => 1 - DisposableDropList.RemainingDropsRate;

    [JsonIgnore]
    public bool ExploreCompleted => DisposableDropList.IsEmpty && RepeatableDropList.IsEmpty;

    protected override void FirstInit()
    {
        AddSlot(9);

        InitState();
        InitDropList();
        if (PlaceData.isInSpacecraft)
        {
            hasCable = true;
            AddCard(CardFactory.CreateCard("渗水裂缝"));
        }
        pressureLevel = PressureLevel.Standard;
    }

    public override void Init()
    {
        base.Init();
        RepeatableDropList.StartUpdating();
        // 每回合结算地点状态
        UpdateManager.Instance.EnvironmentUpdate.AddListener(Update);
    }

    private void InitState()
    {
        // 在室内显示氧气
        // 在室内显示一氧化碳
        if (PlaceData.isIndoor)
        {
            StateDict.Add(EnvironmentStateEnum.Oxygen, new State(UnityEngine.Random.Range(400, 600), 1000));

            var thresholds = new List<StateThreshold>()
            {
                new (-1, 0, "无一氧化碳"),
                new (0, 25, "低浓度"),
                new (25, 50, "中浓度"),
                new (50, 75, "高浓度"),
                new (75, int.MaxValue, "极高浓度"),
            };
            var effects = new List<StateEffect>()
            {
                StateEffect.NoEffect,
                new () { carbonMonoxidePoisoningRate = +0.5f },
                new () { carbonMonoxidePoisoningRate = +1f },
                new () { carbonMonoxidePoisoningRate = +1.7f },
                new () { carbonMonoxidePoisoningRate = +3f },
            };
            StateDict.Add(EnvironmentStateEnum.CarbonMonoxideLevel, new State(0, 100, -0.5f, thresholds, effects, new(), new()));
        }

        // 室温
        stateDict.Add(EnvironmentStateEnum.RoomTemperature, new State(200, 400, normParam: -200));
    }

    private void InitDropList()
    {
        disposableDropList = JsonManager.DeepCopy(CardFactory.GetDisposableDropList(PlaceData.placeType));
        repeatableDropList = JsonManager.DeepCopy(CardFactory.GetRepeatableDropList(PlaceData.placeType));
    }

    private Dictionary<EnvironmentStateEnum, float> temp = new(); // 记录地点状态的当前变化率，防止地点状态的结算顺序影响结算结果

    private void Update()
    {
        temp.Clear();
        foreach (var (type, state) in stateDict)
        {
            if (state.ChangeRate != 0)
            {
                //ChangePlayerState(type, state.ChangeRate);
                temp.Add(type, state.ChangeRate);
            }
        }

        ApplyEnvEffects(temp);
    }

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
            case EnvironmentStateEnum.WaterLevel:
                throw new ArgumentException("修改电力或水平面请通过StateManager.Instance.ChangeElectricity/ChangeWaterLevel方法");
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
                EventManager.Instance.TriggerEvent(EventType.RefreshEnvironmentState, new RefreshEnvironmentStateArgs(placeData.placeType, stateEnum)
                {
                    stateValue = state
                });
                break;
        }
    }

    public void ApplyEnvEffects(Dictionary<EnvironmentStateEnum, float> envEffects)
    {
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
        if (Window != null) Window.RefreshDisplay();
        return hasChanged;
    }

    public override void OnAddCard(Card card)
    {

    }

    public override void OnRemoveCard(Card card)
    {

    }
}
