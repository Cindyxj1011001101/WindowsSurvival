using System.Collections.Generic;
using UnityEngine;
using System;

public class EnvironmentBag : BagBase
{
    public DisposableDropList DisposableDropList { get; private set; } = new();
    public RepeatableDropList RepeatableDropList { get; private set; } = new();

    [Header("探索用时")]
    public int explorationTime;

    [Header("地点数据")]
    [SerializeField] private PlaceData placeData;

    [Header("是否铺设电缆")]
    [SerializeField] private bool hasCable;

    [Header("压强等级")]
    [SerializeField] private PressureLevel pressureLevel;

    // 环境状态字典
    public Dictionary<EnvironmentStateEnum, EnvironmentState> StateDict { get; private set; } = new();

    // 是否铺设电缆
    public bool HasCable => hasCable;

    public PressureLevel PressureLevel => pressureLevel;

    public PlaceData PlaceData => placeData;

    public float DiscoveryDegree => 1 - DisposableDropList.RemainingDropsRate;

    public bool ExploreCompleted => DisposableDropList.IsEmpty && RepeatableDropList.IsEmpty;

    private void Awake()
    {
        // 如果是飞船环境，要考虑水平面变化
        if (placeData.isInSpacecraft)
            EventManager.Instance.AddListener<float>(EventType.ChangeWaterLevel, OnWaterLevelChanged);
    }

    public override void Init()
    {
        InitBag(GameDataManager.Instance.GetEnvironmentBagDataByPlace(placeData.placeType));
    }

    private void OnDestroy()
    {
        // 如果是飞船环境，要考虑水平面变化
        if (placeData.isInSpacecraft)
            EventManager.Instance.RemoveListener<float>(EventType.ChangeWaterLevel, OnWaterLevelChanged);
    }

    protected override void InitBag(BagRuntimeData runtimeData)
    {
        // 初始化背包中的物品，探索度，环境状态值
        base.InitBag(runtimeData);
        var data = (runtimeData as EnvironmentBagRuntimeData);

        if (!data.init)
        {
            InitState();
            InitDropList();
        }
        else
        {
            StateDict = data.environmentStateDict;
            DisposableDropList = data.disposableDropList;
            RepeatableDropList = data.repeatableDropList;
            pressureLevel = data.pressureLevel;
            hasCable = data.hasCable;
        }

        RepeatableDropList.StartUpdating();
    }

    private void InitState()
    {
        // 在室内显示氧气
        if (placeData.isIndoor)
            StateDict.Add(EnvironmentStateEnum.Oxygen, new EnvironmentState(UnityEngine.Random.Range(400, 600), 1000, EnvironmentStateEnum.Oxygen));
    }

    private void InitDropList()
    {
        DisposableDropList = JsonManager.DeepCopy(CardFactory.GetDisposableDropList(placeData.placeType));
        RepeatableDropList = JsonManager.DeepCopy(CardFactory.GetRepeatableDropList(placeData.placeType));
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
                state.CurValue += delta;
                // 刷新前端显示
                EventManager.Instance.TriggerEvent(EventType.RefreshEnvironmentState, new RefreshEnvironmentStateArgs(placeData.placeType, stateEnum)
                {
                    stateValue = state
                });
                break;
        }
    }

    private void OnWaterLevelChanged(float level)
    {
        // 如果当前是水域环境
        if (placeData.isInWater)
        {
            // 如果水平面下降
            if (level < StateManager.Instance.WaterLevel.MaxValue)
                // 变回陆地环境
                placeData.isInWater = false;
        }
        // 如果当前是陆地环境
        else
        {
            if (level >= StateManager.Instance.WaterLevel.MaxValue)
                // 变成水域环境
                placeData.isInWater = true;
        }
    }

    public override bool CanAddCard(Card card)
    {
        return true;
    }

    public override void AddCard(Card card)
    {
        // 如果放不下，就新增格子
        if (!base.CanAddCard(card))
        {
            // 暂定每次新增3个格子
            AddSlot(3);
        }

        base.AddCard(card);

        // 如果剩余格子数量小于3个
        if (UnusedSlotsCount < 3)
        {
            // 暂定每次新增3个格子
            AddSlot(3);
        }
    }

    public override void CompactCards()
    {
        base.CompactCards();
        while (slots.Count - 3 >= 9 && UnusedSlotsCount - 3 >= 3)
        {
            RemoveSlot(slots[^1]);
            RemoveSlot(slots[^1]);
            RemoveSlot(slots[^1]);
        }
    }
}
