using System.Collections.Generic;
using UnityEngine;

public class EnvironmentBag : BagBase
{
    [Header("一次性掉落列表")]
    public DisposableDropList disposableDropList = new();

    [Header("探索用时")]
    public int explorationTime;

    [Header("地点数据")]
    [SerializeField] private PlaceData placeData;

    public PlaceData PlaceData => placeData;

    //[Header("探索度")]
    //private float discoveryDegree;
    //public float DiscoveryDegree => discoveryDegree;
    public float DiscoveryDegree => 1 - disposableDropList.RemainingDropsRate;

    [Header("环境状态")]
    public Dictionary<EnvironmentStateEnum, EnvironmentState> EnvironmentStateDict = new Dictionary<EnvironmentStateEnum, EnvironmentState>();

    protected override void Init()
    {
        InitBag(GameDataManager.Instance.GetEnvironmentBagDataByPlace(placeData.placeType));
        EventManager.Instance.AddListener<ChangeEnvironmentStateArgs>(EventType.CurEnvironmentChangeState, OnEnvironmentChangeState);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<ChangeEnvironmentStateArgs>(EventType.CurEnvironmentChangeState, OnEnvironmentChangeState);
    }

    public void HandeleExplore()
    {
        if (disposableDropList.IsEmpty)
        {
            Debug.Log("探索完全");
            return;
        }

        // 消耗时间
        TimeManager.Instance.AddTime(explorationTime);
        // 掉落卡牌
        foreach (var card in disposableDropList.RandomDrop())
        {
            // 掉落到环境里
            GameManager.Instance.AddCard(card, false);
        }

        // 探索度变化
        //discoveryDegree = 1 - disposableDropList.RemainingDropsRate;
        EventManager.Instance.TriggerEvent(EventType.ChangeDiscoveryDegree, DiscoveryDegree);
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
                EventManager.Instance.TriggerEvent(EventType.RefreshEnvironmentState, args.state);
            }
        }

    }

    protected override void InitBag(BagRuntimeData runtimeData)
    {
        // 初始化背包中的物品，探索度，环境状态值
        base.InitBag(runtimeData);
        var data = (runtimeData as EnvironmentBagRuntimeData);
        //discoveryDegree = data.discoveryDegree;
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
        // 初始化一次性掉落列表
        disposableDropList.Init(data.remainingDrops);
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
}
