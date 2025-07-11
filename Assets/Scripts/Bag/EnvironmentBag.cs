using System.Collections.Generic;
using UnityEngine;

public class EnvironmentBag : BagBase
{
    public CardEvent CardEvent;

    [SerializeField] private PlaceData placeData;

    public PlaceData PlaceData => placeData;

    private float discoveryDegree;
    public float DiscoveryDegree => discoveryDegree;

    protected override void Init()
    {
        InitBag(GameDataManager.Instance.GetEnvironmentBagDataByPlace(placeData.placeType));
        EventManager.Instance.AddListener<ChangeDiscoveryDegreeArgs>(EventType.ChangeDiscoveryDegree, OnDiscoveryDegreeChanged);
    }

    private void OnDiscoveryDegreeChanged(ChangeDiscoveryDegreeArgs args)
    {
        if (args.place == placeData.placeType)
            discoveryDegree = args.discoveryDegree;
    }

    protected override void InitBag(BagRuntimeData runtimeData)
    {
        // 初始化背包中的物品
        base.InitBag(runtimeData);
        var data = (runtimeData as EnvironmentBagRuntimeData);
        discoveryDegree = data.discoveryDegree;
        // 初始化一次性掉落列表
        var e = CardEvent.eventList.Find(c => c is PlaceDropEvent);
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
    }
}
