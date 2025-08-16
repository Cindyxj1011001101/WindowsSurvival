using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class EnvironmentBag : Bag
{
    [JsonProperty] private string placeName;
    [JsonProperty] private bool hasCable;
    [JsonProperty] private PressureLevel pressureLevel;
    [JsonProperty] private DisposableDropList disposableDropList = new();
    [JsonProperty] private RepeatableDropList repeatableDropList = new();
    [JsonProperty] private Dictionary<EnvironmentStateEnum, EnvironmentState> stateDict = new();

    private PlaceData placeData;

    [JsonIgnore] public bool HasCable => hasCable;
    [JsonIgnore] public PressureLevel PressureLevel => pressureLevel;
    [JsonIgnore] public string PlaceName => placeName;
    [JsonIgnore] public DisposableDropList DisposableDropList => disposableDropList;
    [JsonIgnore] public RepeatableDropList RepeatableDropList => repeatableDropList;
    [JsonIgnore] public Dictionary<EnvironmentStateEnum, EnvironmentState> StateDict => stateDict;

    [JsonIgnore]
    public PlaceData PlaceData
    {
        get
        {
            placeData ??= Resources.Load<PlaceData>("ScriptableObject/Place/" + placeName);
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
            hasCable = true;
        pressureLevel = PressureLevel.Standard;
    }

    public override void Init()
    {
        base.Init();
        RepeatableDropList.StartUpdating();
    }
    private void InitState()
    {
        // 在室内显示氧气
        if (PlaceData.isIndoor)
            StateDict.Add(EnvironmentStateEnum.Oxygen, new EnvironmentState(UnityEngine.Random.Range(400, 600), 1000, EnvironmentStateEnum.Oxygen));
    }

    private void InitDropList()
    {
        disposableDropList = JsonManager.DeepCopy(CardFactory.GetDisposableDropList(placeData.placeType));
        repeatableDropList = JsonManager.DeepCopy(CardFactory.GetRepeatableDropList(placeData.placeType));
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
                EventManager.Instance.TriggerEvent(EventType.RefreshEnvironmentState, new RefreshEnvironmentStateArgs(placeData.placeType, stateEnum)
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
