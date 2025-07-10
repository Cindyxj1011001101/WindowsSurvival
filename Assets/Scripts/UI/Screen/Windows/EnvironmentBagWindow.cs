using System;
using UnityEngine.UI;

public class EnvironmentBagWindow : BagWindow
{
    private Text discoveryDegreeText; // 发现度显示
    private Text placeNameText; // 环境袋名称
    private Text placeDetailsText; // 环境袋详情
    private Button discoverButton; // 发现按钮

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

        // 注册发现度变化事件
        EventManager.Instance.AddListener<ChangeDiscoveryDegreeArgs>(EventType.ChangeDiscoveryDegree, OnDicoveryDegreeChanged);
        // 注册环境袋移动事件
        EventManager.Instance.AddListener<EnvironmentBag>(EventType.Move, OnMove);
    }

    private void OnEnable()
    {
        // 当前显示当前环境袋
        EffectResolve.Instance.Move(EffectResolve.Instance.CurEnvironmentBag.PlaceData.placeType);
    }

    protected override void Init()
    {
    }

    /// <summary>
    /// 移动到指定环境袋
    /// </summary>
    private void OnMove(EnvironmentBag curEnvironmentBag)
    {
        // 新的环境信息
        discoveryDegreeText.text = $"{Math.Round(curEnvironmentBag.DiscoveryDegree, 1)} %";
        placeNameText.text = $"{curEnvironmentBag.PlaceData.placeName}";
        placeDetailsText.text = $"{curEnvironmentBag.PlaceData.placeDesc}";
    }

    private void OnDicoveryDegreeChanged(ChangeDiscoveryDegreeArgs args)
    {
        if (args.place == EffectResolve.Instance.CurEnvironmentBag.PlaceData.placeType)
            discoveryDegreeText.text = $"{Math.Round(args.discoveryDegree, 1)} %";
    }

    private void OnDestroy()
    {
        // 移除事件
        EventManager.Instance.RemoveListener<ChangeDiscoveryDegreeArgs>(EventType.ChangeDiscoveryDegree, OnDicoveryDegreeChanged);
        EventManager.Instance.RemoveListener<EnvironmentBag>(EventType.Move, OnMove);
    }
}
