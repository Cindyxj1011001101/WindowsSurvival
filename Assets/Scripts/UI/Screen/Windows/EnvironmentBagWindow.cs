using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PressureLevel
{
    VeryLow = 1,
    Low = 2,
    Standard = 3,
    High = 4,
    VeryHigh = 5
}

public class EnvironmentBagWindow : BagWindow
{
    [SerializeField] private Text discoveryDegreeText; // 探索度显示
    [SerializeField] private Text placeNameText; // 环境名称
    //[SerializeField] private Text placeDetailsText; // 环境详情
    [SerializeField] private Button discoverButton; // 探索按钮
    private Dictionary<EnvironmentStateEnum, GameObject> stateStateSliders = new Dictionary<EnvironmentStateEnum, GameObject>(); // 环境状态显示
    [SerializeField] private StateToggle hasCabbleToggle;

    protected override void Awake()
    {
        base.Awake();

        //discoveryDegreeText = transform.Find("LeftBar/DiscoveryDegree").GetComponent<Text>();
        //placeNameText = transform.Find("TopBar/Name").GetComponent<Text>();
        //placeDetailsText = transform.Find("LeftBar/Details/Text").GetComponent<Text>();
        //discoverButton = transform.Find("LeftBar/DiscoverButton").GetComponent<Button>();
        //stateStateSliders.Add(EnvironmentStateEnum.Electricity, transform.Find("LeftBar/EnvironmentState/BagScrollView/Viewport/Container/Electricity").gameObject);
        //stateStateSliders.Add(EnvironmentStateEnum.Oxygen, transform.Find("LeftBar/EnvironmentState/BagScrollView/Viewport/Container/Oxygen").gameObject);
        //stateStateSliders.Add(EnvironmentStateEnum.Pressure, transform.Find("LeftBar/EnvironmentState/BagScrollView/Viewport/Container/Pressure").gameObject);
        //stateStateSliders.Add(EnvironmentStateEnum.Height, transform.Find("LeftBar/EnvironmentState/BagScrollView/Viewport/Container/Height").gameObject);

        // 注册发现度变化事件
        EventManager.Instance.AddListener<float>(EventType.ChangeDiscoveryDegree, DisplayDiscoveryDegree);
        // 注册环境移动事件
        EventManager.Instance.AddListener<EnvironmentBag>(EventType.Move, OnMove);
        // 注册环境状态变化事件
        EventManager.Instance.AddListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnEnvironmentStateChanged);
    }

    private void OnDestroy()
    {
        // 移除事件
        EventManager.Instance.RemoveListener<float>(EventType.ChangeDiscoveryDegree, DisplayDiscoveryDegree);
        EventManager.Instance.RemoveListener<EnvironmentBag>(EventType.Move, OnMove);
        EventManager.Instance.RemoveListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnEnvironmentStateChanged);
    }

    protected override void Init()
    {
        //GameManager.Instance.Move(GameManager.Instance.CurEnvironmentBag.PlaceData.placeType);
    }

    /// <summary>
    /// 移动到指定环境袋
    /// </summary>
    private void OnMove(EnvironmentBag curEnvironmentBag)
    {
        // 电力都显示

        // 压强都显示

        // 在飞船内显示水平面高度

        // 在室内显示氧气

        // 是否铺设电缆都显示


        // 新的环境信息
        foreach (var state in stateStateSliders)
        {
            //室内显示氧气
            if (curEnvironmentBag.PlaceData.isIndoor)
            {
                stateStateSliders[EnvironmentStateEnum.Oxygen].SetActive(true);
            }
            else
            {
                stateStateSliders[EnvironmentStateEnum.Oxygen].SetActive(false);
            }
            //飞船中显示水平面高度
            if (curEnvironmentBag.PlaceData.isInSpacecraft)
            {
                stateStateSliders[EnvironmentStateEnum.WaterLevel].SetActive(true);
            }
            else
            {
                stateStateSliders[EnvironmentStateEnum.WaterLevel].SetActive(false);
            }
            OnEnvironmentStateChanged(new RefreshEnvironmentStateArgs(curEnvironmentBag.PlaceData.placeType, state.Key));
        }
        discoveryDegreeText.text = $"{Math.Round(curEnvironmentBag.DiscoveryDegree, 3) * 100} %";
        placeNameText.text = $"{curEnvironmentBag.PlaceData.placeName}";
        //placeDetailsText.text = $"{curEnvironmentBag.PlaceData.placeDesc}";
        // 探索事件
        discoverButton.onClick.RemoveAllListeners();
        discoverButton.onClick.AddListener(GameManager.Instance.HandleExplore);
    }

    /// <summary>
    /// 单个环境状态变化UI刷新
    /// </summary>
    private void OnEnvironmentStateChanged(RefreshEnvironmentStateArgs args)
    {
        // 不是当前地点的值变化不显示
        if (args.place != GameManager.Instance.CurEnvironmentBag.PlaceData.placeType) return;

        //if (stateStateSliders.ContainsKey(args.state) && GameManager.Instance.CurEnvironmentBag.stateDict.ContainsKey(args.state))
        //{
        //    //float resultValue = 0;
        //    //if (args.state == EnvironmentStateEnum.Electricity)
        //    //{
        //    //    resultValue = StateManager.Instance.Electricity / GameManager.Instance.CurEnvironmentBag.EnvironmentStateDict[args.state].MaxValue;
        //    //    stateStateSliders[args.state].GetComponentInChildren<Slider>().value = resultValue;
        //    //    stateStateSliders[args.state].transform.Find("StateNum").GetComponent<TMP_Text>().text =
        //    //    StateManager.Instance.Electricity.ToString("f1") + "/"
        //    //    + GameManager.Instance.CurEnvironmentBag.EnvironmentStateDict[args.state].MaxValue.ToString();
        //    //}
        //    //else if (args.state == EnvironmentStateEnum.Height)
        //    //{
        //    //    resultValue = StateManager.Instance.WaterLevel / GameManager.Instance.CurEnvironmentBag.EnvironmentStateDict[args.state].MaxValue;
        //    //    stateStateSliders[args.state].GetComponentInChildren<Slider>().value = resultValue;
        //    //    stateStateSliders[args.state].transform.Find("StateNum").GetComponent<TMP_Text>().text =
        //    //    StateManager.Instance.WaterLevel.ToString("f1") + "/"
        //    //    + GameManager.Instance.CurEnvironmentBag.EnvironmentStateDict[args.state].MaxValue.ToString();
        //    //}
        //    //else if (args.state == EnvironmentStateEnum.Pressure)
        //    //{
        //    //    resultValue = GameManager.Instance.CurEnvironmentBag.EnvironmentStateDict[args.state].curValue
        //    //    / GameManager.Instance.CurEnvironmentBag.EnvironmentStateDict[args.state].MaxValue;
        //    //    stateStateSliders[args.state].GetComponentInChildren<Slider>().value = resultValue;
        //    //    string showText = "";
        //    //    switch (GameManager.Instance.CurEnvironmentBag.EnvironmentStateDict[args.state].curValue)
        //    //    {
        //    //        case 0:
        //    //            showText = "极低";
        //    //            break;
        //    //        case 1:
        //    //            showText = "低";
        //    //            break;
        //    //        case 2:
        //    //            showText = "标准";
        //    //            break;
        //    //        case 3:
        //    //            showText = "高";
        //    //            break;
        //    //        case 4:
        //    //            showText = "极高";
        //    //            break;
        //    //    }
        //    //    stateStateSliders[args.state].transform.Find("StateNum").GetComponent<TMP_Text>().text = showText;
        //    //}
        //    //else
        //    //{
        //    //    resultValue = GameManager.Instance.CurEnvironmentBag.EnvironmentStateDict[args.state].curValue
        //    //    / GameManager.Instance.CurEnvironmentBag.EnvironmentStateDict[args.state].MaxValue;
        //    //    stateStateSliders[args.state].GetComponentInChildren<Slider>().value = resultValue;
        //    //    stateStateSliders[args.state].transform.Find("StateNum").GetComponent<TMP_Text>().text =
        //    //    GameManager.Instance.CurEnvironmentBag.EnvironmentStateDict[args.state].curValue.ToString("f1") + "/"
        //    //    + GameManager.Instance.CurEnvironmentBag.EnvironmentStateDict[args.state].MaxValue.ToString();
        //    //}
        //}
    }

    private void DisplayDiscoveryDegree(float degree)
    {
        discoveryDegreeText.text = $"{Math.Round(degree, 3) * 100} %";
    }
}
