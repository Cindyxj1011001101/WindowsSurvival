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

    private void Awake()
    {
        // 如果是飞船环境，要考虑水平面变化
        if (placeData.isInSpacecraft)
            EventManager.Instance.AddListener<float>(EventType.ChangeWaterLevel, OnWaterLevelChanged);

        InitBag(GameDataManager.Instance.GetEnvironmentBagDataByPlace(placeData.placeType));
    }

    protected override void Init()
    {
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

        // 读取掉落列表
        DisposableDropList = data.disposableDropList;
        RepeatableDropList = data.repeatableDropList;
        RepeatableDropList.StartUpdating();

        // 读取环境状态
        StateDict = data.environmentStateDict;

        //如果是开局进入，则初始化环境状态
        if (StateDict.Count == 0)
            InitState();
    }

    private void InitState()
    {
        // 电力都显示
        StateDict.Add(EnvironmentStateEnum.Electricity, StateManager.Instance.Electricity);

        // 在飞船内显示水平面高度
        if (placeData.isInSpacecraft)
            StateDict.Add(EnvironmentStateEnum.WaterLevel, StateManager.Instance.WaterLevel);

        // 在室内显示氧气
        if (placeData.isIndoor)
            StateDict.Add(EnvironmentStateEnum.Oxygen, new EnvironmentState(UnityEngine.Random.Range(400, 600), 1000, EnvironmentStateEnum.Oxygen));
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
            case EnvironmentStateEnum.Oxygen:
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
            default:
                throw new ArgumentException("修改是否铺设电缆或压强请通过ChangeHasCable/ChangePressureLevel方法");
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
    }

    public override List<(CardSlot, int)> GetSlotsCanAddCard(Card card, int count)
    {
        List<(CardSlot, int)> result = new();

        int leftCount = count; // 剩余要添加的数量

        // 优先堆叠，卡牌格按照已堆叠数量降序排序，即优先堆满
        foreach (var slot in GetSlotsByCardId(card.cardId, false))
        {
            if (leftCount <= 0) return result;

            int remainingCapacity = slot.GetRemainingCapacity(card);
            if (remainingCapacity > 0)
            {
                result.Add((slot, Mathf.Min(remainingCapacity, leftCount)));
                leftCount -= remainingCapacity;
            }
        }

        // 如果还有要添加的卡牌
        // 这里不能while true，因为上面的循环可能正常结束但是leftCount<=0
        while (leftCount > 0)
        {
            // 找空位
            foreach (var slot in slots)
            {
                if (slot.IsEmpty)
                {
                    result.Add((slot, Mathf.Min(card.maxStackNum, leftCount)));
                    leftCount -= card.maxStackNum;
                }

                if (leftCount <= 0) return result;
            }
            // 没有return说明还有要添加的卡牌
            // 那么新增3个空卡牌格
            AddSlot(3);
        }

        return result;
    }
}
