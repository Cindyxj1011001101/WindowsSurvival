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
        base.InitBag(runtimeData);
        var data = (runtimeData as EnvironmentBagRuntimeData);
        discoveryDegree = data.discoveryDegree;
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
