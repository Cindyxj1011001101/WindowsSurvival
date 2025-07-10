using UnityEngine.UI;

public class EnvironmentBagWindow : BagWindow
{
    private Text discoveryDegreeText; // 探索度显示
    private Text placeNameText; // 地点名称
    private Text placeDetailsText; // 地点详情
    private Button discoverButton; // 探索按钮

    //private PlaceEnum currentPlace;

    //private Dictionary<PlaceEnum, BagBase> environmentBags = new();

    protected override void Awake()
    {
        base.Awake();

        discoveryDegreeText = transform.Find("LeftBar/DiscoveryDegree").GetComponent<Text>();
        placeNameText = transform.Find("TopBar/Name").GetComponent<Text>();
        placeDetailsText = transform.Find("LeftBar/Details/Text").GetComponent<Text>();
        discoverButton = transform.Find("LeftBar/DiscoverButton").GetComponent<Button>();

        discoverButton.onClick.AddListener(() =>
        {
            EffectResolve.Instance.ResolveExplore();
        });

        // 注册探索度变化事件
        EventManager.Instance.AddListener<ChangeDiscoveryDegreeArgs>(EventType.ChangeDiscoveryDegree, OnDicoveryDegreeChanged);
        // 注册地点移动事件
        EventManager.Instance.AddListener<EnvironmentBag>(EventType.Move, OnMove);
    }

    private void OnEnable()
    {
        // 窗口激活时，显示当前地点
        EffectResolve.Instance.Move(EffectResolve.Instance.CurEnvironmentBag.PlaceData.placeType);
    }

    protected override void Init()
    {
    }

    /// <summary>
    /// 移动到指定地点
    /// </summary>
    private void OnMove(EnvironmentBag curEnvironmentBag)
    {
        // 更新地点信息
        discoveryDegreeText.text = $"{curEnvironmentBag.DiscoveryDegree} %";
        placeNameText.text = $"{curEnvironmentBag.PlaceData.placeName}";
        placeDetailsText.text = $"{curEnvironmentBag.PlaceData.placeDesc}";
    }

    private void OnDicoveryDegreeChanged(ChangeDiscoveryDegreeArgs args)
    {
        if (args.place == EffectResolve.Instance.CurEnvironmentBag.PlaceData.placeType)
            discoveryDegreeText.text = $"{args.discoveryDegree} %";
    }

    private void OnDestroy()
    {
        // 移除注册事件
        EventManager.Instance.RemoveListener<ChangeDiscoveryDegreeArgs>(EventType.ChangeDiscoveryDegree, OnDicoveryDegreeChanged);
        EventManager.Instance.RemoveListener<EnvironmentBag>(EventType.Move, OnMove);
    }
}
