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
    [SerializeField] private UIStateSlider discoveryDegreeSlider; // 探索度显示
    [SerializeField] private Text placeNameText; // 环境名称
    [SerializeField] private Image environmentImage; // 环境图片
    [SerializeField] private HoverableButton exploreButton; // 探索按钮
    [SerializeField] private RectTransform stateLayout;
    [SerializeField] private RectTransform frontCard;

    private UIStateToggle hasCabbleToggle; // 是否铺设电缆
    private UIPressureLevel pressureLevel; // 压强等级
    private Dictionary<EnvironmentStateEnum, UIStateSlider> stateSliders = new(); // 环境状态显示

    private HoverTipController hoveredTipController;

    public RectTransform FrontCard => frontCard;

    protected override void Awake()
    {
        base.Awake();

        // 注册探索度变化事件
        EventManager.Instance.AddListener<(float, bool)>(EventType.ChangeDiscoveryDegree, DisplayDiscoveryDegree);
        // 注册环境移动事件
        EventManager.Instance.AddListener<EnvironmentBag>(EventType.Move, OnMove);
        // 注册环境状态变化事件
        EventManager.Instance.AddListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnEnvironmentStateChanged);
    }

    private void OnDestroy()
    {
        // 移除事件
        EventManager.Instance.RemoveListener<(float, bool)>(EventType.ChangeDiscoveryDegree, DisplayDiscoveryDegree);
        EventManager.Instance.RemoveListener<EnvironmentBag>(EventType.Move, OnMove);
        EventManager.Instance.RemoveListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnEnvironmentStateChanged);
    }

    protected override void Init()
    {
        hoveredTipController = exploreButton.gameObject.AddComponent<HoverTipController>();
        hoveredTipController.onPointerEnter.AddListener(() =>
        {
            var (desc, time, playerEffects, envEffects) = GameManager.Instance.GetExploreEffects();
            hoveredTipController.SetTip(desc, time, playerEffects, envEffects);
        });
    }

    /// <summary>
    /// 移动到指定环境
    /// </summary>
    private void OnMove(EnvironmentBag curEnvironmentBag)
    {
        stateSliders.Clear();

        MonoUtility.DestroyAllChildren(stateLayout);

        // 压强都显示
        pressureLevel = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Controls/EnvironmentState/PressureLevel"), stateLayout).GetComponent<UIPressureLevel>();
        pressureLevel.SetValue(curEnvironmentBag.PressureLevel);

        // 是否铺设电缆都显示
        hasCabbleToggle = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Controls/EnvironmentState/HasCable"), stateLayout).GetComponent<UIStateToggle>();
        hasCabbleToggle.SetStateName("铺设电缆");
        hasCabbleToggle.SetValue(curEnvironmentBag.HasCable);

        // 铺设电缆才显示电力
        UIStateSlider slider;
        if (curEnvironmentBag.HasCable)
        {
            slider = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Controls/EnvironmentState/Electricity"), stateLayout).GetComponent<UIStateSlider>();
            slider.SetValue(StateManager.Instance.Electricity);
            stateSliders.Add(EnvironmentStateEnum.Electricity, slider);
        }

        // 在飞船内显示水平面高度
        if (curEnvironmentBag.PlaceData.isInSpacecraft)
        {
            slider = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Controls/EnvironmentState/WaterLevel"), stateLayout).GetComponent<UIStateSlider>();
            slider.SetValue(StateManager.Instance.WaterLevel);
            stateSliders.Add(EnvironmentStateEnum.WaterLevel, slider);
        }

        // 其他状态显示
        foreach (var (state, value) in curEnvironmentBag.StateDict)
        {
            slider = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Controls/EnvironmentState/" + state.ToString()), stateLayout).GetComponent<UIStateSlider>();
            slider.SetValue(value);
            stateSliders.Add(state, slider);
        }

        MonoUtility.UpdateLayoutSize(stateLayout.GetComponent<VerticalLayoutGroup>());

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
            case EnvironmentStateEnum.HasCable:
                hasCabbleToggle.SetValue(args.hasCable);
                break;
            case EnvironmentStateEnum.PressureLevel:
                pressureLevel.SetValue(args.pressureLevel);
                break;
            default:
                // 不存在这个状态不显示
                if (!stateSliders.ContainsKey(args.stateEnum)) return;
                stateSliders[args.stateEnum].SetValue(args.stateValue);
                break;
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

            // 不再显示探索提示
            hoveredTipController.HideTip();
            hoveredTipController.enabled = false;
        }
        else if (args.degree == 1)
        {
            // 深入探索
            exploreButton.normalImage.gameObject.SetActive(true);
            exploreButton.Interactable = true;
            exploreButton.onClick.AddListener(() =>
            {
                GameManager.Instance.HandleExplore(out string tip);
                CardTweenUtility.ShowTip(tip, exploreButton.transform.position + (exploreButton.transform as RectTransform).sizeDelta.y * 0.55f * Vector3.up, ColorManager.yellow);
            });
            text.text = "深入探索";
            text.color = ColorManager.white;

            hoveredTipController.enabled = true;
        }
        else
        {
            exploreButton.normalImage.gameObject.SetActive(true);
            exploreButton.Interactable = true;
            exploreButton.onClick.AddListener(() =>
            {
                GameManager.Instance.HandleExplore(out string tip);
                CardTweenUtility.ShowTip(tip, exploreButton.transform.position + (exploreButton.transform as RectTransform).sizeDelta.y * 0.55f * Vector3.up, ColorManager.yellow);
            });
            text.text = "开始探索";
            text.color = ColorManager.white;

            hoveredTipController.enabled = true;
        }

        // 显示牌堆数量
        frontCard.anchoredPosition = new Vector2(frontCard.anchoredPosition.x, -Mathf.FloorToInt(args.degree * 4) * 4);
    }
}
