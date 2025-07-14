using System.Collections.Generic;
using UnityEngine;

public class EnvironmentBag : BagBase
{
    [Header("探索事件")]
    [SerializeField] private CardEvent exploreEvent;

    public CardEvent ExploreEvent => exploreEvent;

    [Header("地点数据")]
    [SerializeField] private PlaceData placeData;

    public PlaceData PlaceData => placeData;

    [Header("探索度")]
    private float discoveryDegree;
    public float DiscoveryDegree => discoveryDegree;

    [Header("环境状态")]
    public Dictionary<EnvironmentStateEnum, EnvironmentState> EnvironmentStateDict = new Dictionary<EnvironmentStateEnum, EnvironmentState>();


    protected override void Init()
    {
        InitBag(GameDataManager.Instance.GetEnvironmentBagDataByPlace(placeData.placeType));
        EventManager.Instance.AddListener<ChangeDiscoveryDegreeArgs>(EventType.ChangeDiscoveryDegree, OnDiscoveryDegreeChanged);
        EventManager.Instance.AddListener<ChangeEnvironmentStateArgs>(EventType.CurEnvironmentChangeState, OnEnvironmentChangeState);
    }

    private void OnDiscoveryDegreeChanged(ChangeDiscoveryDegreeArgs args)
    {
        if (args.place == placeData.placeType)
            discoveryDegree = args.discoveryDegree;
    }

    private void OnEnvironmentChangeState(ChangeEnvironmentStateArgs args)
    {
        if(EnvironmentStateDict.ContainsKey(args.state))
        {
            EnvironmentStateDict[args.state].curValue += args.value;
            if(EnvironmentStateDict[args.state].curValue>=EnvironmentStateDict[args.state].MaxValue)
            {
                EnvironmentStateDict[args.state].curValue=EnvironmentStateDict[args.state].MaxValue;
            }
            if(EnvironmentStateDict[args.state].curValue<=0)
            {
                EnvironmentStateDict[args.state].curValue=0;
            }
            EventManager.Instance.TriggerEvent(EventType.RefreshEnvironmentState, args.state);
        }
    }

    protected override void InitBag(BagRuntimeData runtimeData)
    {
        // 初始化背包中的物品，探索度，环境状态值
        base.InitBag(runtimeData);
        var data = (runtimeData as EnvironmentBagRuntimeData);
        discoveryDegree = data.discoveryDegree;
        EnvironmentStateDict = new Dictionary<EnvironmentStateEnum, EnvironmentState>(data.environmentStateDict);
        //如果是开局进入，则初始化环境状态
        if (EnvironmentStateDict.Count == 0){
            EnvironmentStateDict=StateManager.Instance.InitEnvironmentStateDict();
        }
        // 初始化一次性掉落列表
        var e = exploreEvent.eventList.Find(c => c is PlaceDropEvent);
        (e as PlaceDropEvent).curOnceDropList = data.disposableDropList;
    }

    public override void AddCard(CardInstance card)
    {   
        // 如果放不下，就新增格子
        if (!CanAddCard(card))
        {
            // 暂定每次新增3个格子
            AddSlot(3);
        }
        base.AddCard(card);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<ChangeDiscoveryDegreeArgs>(EventType.ChangeDiscoveryDegree, OnDiscoveryDegreeChanged);
        EventManager.Instance.RemoveListener<ChangeEnvironmentStateArgs>(EventType.CurEnvironmentChangeState, OnEnvironmentChangeState);
    }
}
