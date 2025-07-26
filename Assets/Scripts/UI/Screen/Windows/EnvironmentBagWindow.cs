using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    [SerializeField] private StateSlider discoveryDegreeSlider; // 探索度显示
    [SerializeField] private Text placeNameText; // 环境名称
    [SerializeField] private Image environmentImage; // 环境图片
    [SerializeField] private HoverableButton exploreButton; // 探索按钮
    [SerializeField] private RectTransform stateLayout;
    [SerializeField] private RectTransform dropCardLayout;

    private StateToggle hasCabbleToggle; // 是否铺设电缆
    private StatePressureLevel pressureLevel; // 压强等级
    private Dictionary<EnvironmentStateEnum, StateSlider> stateSliders = new(); // 环境状态显示

    protected override void Awake()
    {
        base.Awake();

        // 注册探索度变化事件
        EventManager.Instance.AddListener<(float, bool)>(EventType.ChangeDiscoveryDegree, DisplayDiscoveryDegree);
        // 注册环境移动事件
        EventManager.Instance.AddListener<EnvironmentBag>(EventType.Move, OnMove);
        // 注册环境状态变化事件
        EventManager.Instance.AddListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnEnvironmentStateChanged);
        // 注册探索掉落卡牌事件
        EventManager.Instance.AddListener<List<Card>>(EventType.ExploreDropCards, OnExploreDropCards);
    }

    private void OnDestroy()
    {
        // 移除事件
        EventManager.Instance.RemoveListener<(float, bool)>(EventType.ChangeDiscoveryDegree, DisplayDiscoveryDegree);
        EventManager.Instance.RemoveListener<EnvironmentBag>(EventType.Move, OnMove);
        EventManager.Instance.RemoveListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnEnvironmentStateChanged);
        EventManager.Instance.RemoveListener<List<Card>>(EventType.ExploreDropCards, OnExploreDropCards);
    }

    protected override void Init()
    {

    }

    /// <summary>
    /// 移动到指定环境
    /// </summary>
    private void OnMove(EnvironmentBag curEnvironmentBag)
    {
        stateSliders.Clear();

        MonoUtility.DestroyAllChildren(stateLayout);

        // 压强都显示
        pressureLevel = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Controls/State/PressureLevel"), stateLayout).GetComponent<StatePressureLevel>();
        pressureLevel.SetValue(curEnvironmentBag.PressureLevel);

        // 是否铺设电缆都显示
        hasCabbleToggle = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Controls/State/ToggleState"), stateLayout).GetComponent<StateToggle>();
        hasCabbleToggle.SetStateName("铺设电缆");
        hasCabbleToggle.SetValue(curEnvironmentBag.HasCable);

        // 铺设电缆才显示电力
        StateSlider slider;
        if (curEnvironmentBag.HasCable)
        {
            slider = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Controls/State/SliderState"), stateLayout).GetComponent<StateSlider>();
            slider.SetStateName("电力");
            slider.displayPercentage = false;
            slider.SetValue(StateManager.Instance.Electricity);
            stateSliders.Add(EnvironmentStateEnum.Electricity, slider);
        }

        // 在飞船内显示水平面高度
        if (curEnvironmentBag.PlaceData.isInSpacecraft)
        {
            slider = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Controls/State/SliderState"), stateLayout).GetComponent<StateSlider>();
            slider.SetStateName("水平面");
            slider.displayPercentage = true;
            slider.SetValue(StateManager.Instance.WaterLevel);
            stateSliders.Add(EnvironmentStateEnum.WaterLevel, slider);
        }

        // 在室内显示氧气
        if (curEnvironmentBag.StateDict.ContainsKey(EnvironmentStateEnum.Oxygen))
        {
            slider = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Controls/State/SliderState"), stateLayout).GetComponent<StateSlider>();
            slider.SetStateName("氧气");
            slider.displayPercentage = false;
            slider.SetValue(curEnvironmentBag.StateDict[EnvironmentStateEnum.Oxygen]);
            stateSliders.Add(EnvironmentStateEnum.Oxygen, slider);
        }

        MonoUtility.UpdateContainerHeight(stateLayout.GetComponent<VerticalLayoutGroup>());

        // 显示环境名称
        placeNameText.text = $"{curEnvironmentBag.PlaceData.placeName}";
        // 探索事件
        DisplayDiscoveryDegree((curEnvironmentBag.DiscoveryDegree, curEnvironmentBag.ExploreCompleted));

        // 显示图片
        environmentImage.sprite = curEnvironmentBag.PlaceData.placeImage;
        environmentImage.SetNativeSize();
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
                pressureLevel.SetValue(args.pressureLevel);
                break;
            default:
                throw new ArgumentException("未知状态改变: " + args.stateEnum.ToString());
        }
    }

    private void DisplayDiscoveryDegree((float degree, bool completed) args)
    {
        // 显示探索度
        discoveryDegreeSlider.SetValue(args.degree, 1);

        // 显示探索按钮
        exploreButton.onClick.RemoveAllListeners(); // 清除之前的监听器
        var text = exploreButton.GetComponentInChildren<Text>();
        if (args.completed)
        {
            exploreButton.normalImage.gameObject.SetActive(false);
            exploreButton.Interactable = false;
            text.text = "探索完成";
            text.color = ColorManager.cyan;
        }
        else if (args.degree == 1)
        {
            // 深入探索
            exploreButton.normalImage.gameObject.SetActive(true);
            exploreButton.Interactable = true;
            exploreButton.onClick.AddListener(GameManager.Instance.HandleExplore);
            text.text = "深入探索";
            text.color = ColorManager.white;
        }
        else
        {
            exploreButton.normalImage.gameObject.SetActive(true);
            exploreButton.Interactable = true;
            exploreButton.onClick.AddListener(GameManager.Instance.HandleExplore);
            text.text = "开始探索";
            text.color = ColorManager.white;
        }

        // 显示牌堆数量
        int toDisplayCount = Mathf.CeilToInt((1 - args.degree) * dropCardLayout.childCount);
        for (int i = 0; i < dropCardLayout.childCount; i++)
        {
            dropCardLayout.GetChild(i).gameObject.SetActive(i < toDisplayCount);
        }
    }

    private async void OnExploreDropCards(List<Card> cards)
    {
        foreach (var card in cards)
        {
            CardMoveTween.MoveCard(
               card,
               1,
               dropCardLayout.transform.position,
               card.Slot.transform.position,
               0.2f,
               null,
               () =>
               {
                   // 再刷新显示
                   card.Slot.RefreshCurrentDisplay();
               });
            await Task.Delay(100); // 等待100毫秒，避免卡牌移动过快
        }
    }
}
