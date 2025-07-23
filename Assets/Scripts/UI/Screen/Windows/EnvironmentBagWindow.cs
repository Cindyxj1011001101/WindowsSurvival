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
    [SerializeField] private StateToggle hasCabbleToggle;
    [SerializeField] private StateText pressureLevelText;
    [SerializeField] private RectTransform stateLayout;

    private Dictionary<EnvironmentStateEnum, StateSlider> stateSliders = new(); // 环境状态显示

    protected override void Awake()
    {
        base.Awake();

        // 注册探索度变化事件
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
        
    }

    /// <summary>
    /// 移动到指定环境袋
    /// </summary>
    private void OnMove(EnvironmentBag curEnvironmentBag)
    {
        stateSliders.Clear();

        MonoUtility.DestroyAllChildren(stateLayout);

        // 是否铺设电缆都显示
        hasCabbleToggle = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Controls/EnvironmentState/ToggleState"), stateLayout).GetComponent<StateToggle>();
        hasCabbleToggle.SetStateName("铺设电缆");
        hasCabbleToggle.SetValue(curEnvironmentBag.HasCable);

        // 电力都显示
        var slider = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Controls/EnvironmentState/SliderState"), stateLayout).GetComponent<StateSlider>();
        slider.SetStateName("电力");
        slider.displayPercentage = false;
        slider.SetValue(StateManager.Instance.Electricity);
        stateSliders.Add(EnvironmentStateEnum.Electricity, slider);

        // 压强都显示
        pressureLevelText = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Controls/EnvironmentState/TextState"), stateLayout).GetComponent<StateText>();
        pressureLevelText.SetStateName("压强");
        pressureLevelText.SetValue(ParsePressureLevel(curEnvironmentBag.PressureLevel));

        // 在飞船内显示水平面高度
        if (curEnvironmentBag.StateDict.ContainsKey(EnvironmentStateEnum.WaterLevel))
        {
            slider = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Controls/EnvironmentState/SliderState"), stateLayout).GetComponent<StateSlider>();
            slider.SetStateName("水平面");
            slider.displayPercentage = true;
            slider.SetValue(StateManager.Instance.WaterLevel);
            stateSliders.Add(EnvironmentStateEnum.WaterLevel, slider);
        }

        // 在室内显示氧气
        if (curEnvironmentBag.StateDict.ContainsKey(EnvironmentStateEnum.Oxygen))
        {
            slider = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Controls/EnvironmentState/SliderState"), stateLayout).GetComponent<StateSlider>();
            slider.SetStateName("氧气");
            slider.displayPercentage = false;
            slider.SetValue(curEnvironmentBag.StateDict[EnvironmentStateEnum.Oxygen]);
            stateSliders.Add(EnvironmentStateEnum.Oxygen, slider);
        }

        // 环境信息
        discoveryDegreeText.text = $"{Math.Round(curEnvironmentBag.DiscoveryDegree, 3) * 100} %";
        placeNameText.text = $"{curEnvironmentBag.PlaceData.placeName}";
        //placeDetailsText.text = $"{curEnvironmentBag.PlaceData.placeDesc}";

        // 探索事件
        discoverButton.onClick.RemoveAllListeners();
        discoverButton.onClick.AddListener(GameManager.Instance.HandleExplore);
    }

    private  string ParsePressureLevel(PressureLevel level)
    {
        return level switch
        {
            PressureLevel.VeryLow => "极低",
            PressureLevel.Low => "低",
            PressureLevel.Standard => "标准",
            PressureLevel.High => "高",
            PressureLevel.VeryHigh => "极高",
            _ => "未知",
        };
    }


    /// <summary>
    /// 单个环境状态变化UI刷新
    /// </summary>
    private void OnEnvironmentStateChanged(RefreshEnvironmentStateArgs args)
    {
        // 不是当前地点的值变化不显示
        if (args.place != GameManager.Instance.CurEnvironmentBag.PlaceData.placeType) return;

        switch (args.stateEnum)
        {
            case EnvironmentStateEnum.Electricity:
            case EnvironmentStateEnum.Oxygen:
            case EnvironmentStateEnum.WaterLevel:
                // 不存在这个状态不显示
                if (!stateSliders.ContainsKey(args.stateEnum)) return;
                stateSliders[args.stateEnum].SetValue(args.stateValue);
                break;
            case EnvironmentStateEnum.HasCable:
                hasCabbleToggle.SetValue(args.hasCable);
                break;
            case EnvironmentStateEnum.PressureLevel:
                pressureLevelText.SetValue(ParsePressureLevel(args.pressureLevel));
                break;
            default:
                throw new ArgumentException("未知状态改变: " + args.stateEnum.ToString());
        }
    }

    private void DisplayDiscoveryDegree(float degree)
    {
        discoveryDegreeText.text = $"{Math.Round(degree, 3) * 100} %";
    }
}
