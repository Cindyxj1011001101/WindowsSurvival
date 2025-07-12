using System;
using UnityEngine.UI;

public class EnvironmentBagWindow : BagWindow
{
    private Text discoveryDegreeText; // 探索度显示
    private Text placeNameText; // 环境名称
    private Text placeDetailsText; // 环境详情
    private Button discoverButton; // 探索按钮

    protected override void Awake()
    {
        base.Awake();

        discoveryDegreeText = transform.Find("LeftBar/DiscoveryDegree").GetComponent<Text>();
        placeNameText = transform.Find("TopBar/Name").GetComponent<Text>();
        placeDetailsText = transform.Find("LeftBar/Details/Text").GetComponent<Text>();
        discoverButton = transform.Find("LeftBar/DiscoverButton").GetComponent<Button>();

        discoverButton.onClick.AddListener(() =>
        {
            GameManager.Instance.HandleExplore();
        });

        // 注册发现度变化事件
        EventManager.Instance.AddListener<ChangeDiscoveryDegreeArgs>(EventType.ChangeDiscoveryDegree, OnDicoveryDegreeChanged);
        // 注册环境袋移动事件
        EventManager.Instance.AddListener<EnvironmentBag>(EventType.Move, OnMove);
    }

    protected override void Init()
    {
        GameManager.Instance.Move(GameManager.Instance.CurEnvironmentBag.PlaceData.placeType);
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
        if (args.place == GameManager.Instance.CurEnvironmentBag.PlaceData.placeType)
            discoveryDegreeText.text = $"{Math.Round(args.discoveryDegree, 1)} %";
    }

    private void OnDestroy()
    {
        // 移除事件
        EventManager.Instance.RemoveListener<ChangeDiscoveryDegreeArgs>(EventType.ChangeDiscoveryDegree, OnDicoveryDegreeChanged);
        EventManager.Instance.RemoveListener<EnvironmentBag>(EventType.Move, OnMove);
    }
}
