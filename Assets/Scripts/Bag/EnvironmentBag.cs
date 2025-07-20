using System.Collections.Generic;
using UnityEngine;

public class EnvironmentBag : BagBase
{
    [Header("一次性掉落列表")]
    public DisposableDropList disposableDropList = new();

    [Header("重复掉落列表")]
    public RepeatableDropList repeatableDropList = new();

    [Header("探索用时")]
    public int explorationTime;

    [Header("地点数据")]
    [SerializeField] private PlaceData placeData;

    public PlaceData PlaceData => placeData;

    public float DiscoveryDegree => 1 - disposableDropList.RemainingDropsRate;

    [Header("环境状态")]
    public Dictionary<EnvironmentStateEnum, EnvironmentState> EnvironmentStateDict = new();

    protected override void Init()
    {
        InitBag(GameDataManager.Instance.GetEnvironmentBagDataByPlace(placeData.placeType));
        EventManager.Instance.AddListener<ChangeEnvironmentStateArgs>(EventType.CurEnvironmentChangeState, OnEnvironmentChangeState);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<ChangeEnvironmentStateArgs>(EventType.CurEnvironmentChangeState, OnEnvironmentChangeState);
    }

    protected override void InitBag(BagRuntimeData runtimeData)
    {
        // 初始化背包中的物品，探索度，环境状态值
        base.InitBag(runtimeData);
        var data = (runtimeData as EnvironmentBagRuntimeData);
        EnvironmentStateDict = new Dictionary<EnvironmentStateEnum, EnvironmentState>(data.environmentStateDict);
        //如果是开局进入，则初始化环境状态
        if (EnvironmentStateDict.Count == 0)
        {
            EnvironmentStateDict = StateManager.Instance.InitEnvironmentStateDict();
        }
        foreach (var state in EnvironmentStateDict)
        {
            EventManager.Instance.TriggerEvent(EventType.RefreshEnvironmentState, new RefreshEnvironmentStateArgs(this.placeData.placeType, state.Key));
        }
        EventManager.Instance.TriggerEvent(EventType.RefreshEnvironmentState, new RefreshEnvironmentStateArgs(this.placeData.placeType, EnvironmentStateEnum.Electricity));
        // 初始化掉落列表
        disposableDropList = data.disposableDropList;
        repeatableDropList = data.repeatableDropList;
        repeatableDropList.StartUpdating();
    }

    //当前环境状态变化(除电力以外的数值变化)
    private void OnEnvironmentChangeState(ChangeEnvironmentStateArgs args)
    {
        if (args.place == placeData.placeType)
        {
            if (EnvironmentStateDict.ContainsKey(args.state))
            {
                EnvironmentStateDict[args.state].curValue += args.value;
                if (EnvironmentStateDict[args.state].curValue >= EnvironmentStateDict[args.state].MaxValue)
                {
                    EnvironmentStateDict[args.state].curValue = EnvironmentStateDict[args.state].MaxValue;
                }
                if (EnvironmentStateDict[args.state].curValue <= 0)
                {
                    EnvironmentStateDict[args.state].curValue = 0;
                }
                //前端UI刷新
                EventManager.Instance.TriggerEvent(EventType.RefreshEnvironmentState, new RefreshEnvironmentStateArgs(placeData.placeType, args.state));
            }
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
