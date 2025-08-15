using DG.Tweening;
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
    [Header("抖动参数")]
    [Header("位置")]
    public float pDuration;
    public Vector3 pStrength;
    public int pVibrato;

    [Header("旋转")]
    public float rDuration;
    public Vector3 rStrength;
    public int rVibrato;


    [Header("")]
    [SerializeField] private UIStateSlider discoveryDegreeSlider; // 探索度显示
    [SerializeField] private Text placeNameText; // 环境名称
    [SerializeField] private Image environmentImage; // 环境图片
    [SerializeField] private HoverableButton exploreButton; // 探索按钮
    [SerializeField] private RectTransform stateLayout;
    [SerializeField] private RectTransform envCardTransform;

    private UIStateToggle hasCabbleToggle; // 是否铺设电缆
    private UIPressureLevel pressureLevel; // 压强等级
    private Dictionary<EnvironmentStateEnum, UIStateSlider> stateSliders = new(); // 环境状态显示

    private HoverTipController hoveredTipController;

    protected override void Awake()
    {
        base.Awake();

        hoveredTipController = exploreButton.gameObject.AddComponent<HoverTipController>();
        hoveredTipController.onPointerEnter.AddListener(() =>
        {
            var (desc, time, playerEffects, envEffects) = GameManager.Instance.GetExploreEffects();
            if (GameManager.Instance.CanMoveExplore())
                hoveredTipController.SetTip(desc, time, playerEffects, envEffects);
            else
                hoveredTipController.SetTip(desc);
        });

        // 注册探索度变化事件
        EventManager.Instance.AddListener<(float, bool)>(EventType.ChangeDiscoveryDegree, DisplayDiscoveryDegree);
        // 注册环境移动事件
        EventManager.Instance.AddListener<EnvironmentBag>(EventType.Move, DisplayBag);
        // 注册环境状态变化事件
        EventManager.Instance.AddListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnEnvironmentStateChanged);
        // 玩家背包卡牌变化
        EventManager.Instance.AddListener<ChangePlayerBagCardsArgs>(EventType.ChangePlayerBagCards, OnPlayerBagCardsChanged);
    }

    private void OnDestroy()
    {
        // 移除事件
        EventManager.Instance.RemoveListener<(float, bool)>(EventType.ChangeDiscoveryDegree, DisplayDiscoveryDegree);
        EventManager.Instance.RemoveListener<EnvironmentBag>(EventType.Move, DisplayBag);
        EventManager.Instance.RemoveListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnEnvironmentStateChanged);
        EventManager.Instance.RemoveListener<ChangePlayerBagCardsArgs>(EventType.ChangePlayerBagCards, OnPlayerBagCardsChanged);
    }

    private void OnPlayerBagCardsChanged(ChangePlayerBagCardsArgs args)
    {
        DisplayDiscoveryDegree((GameManager.Instance.CurEnvironmentBag.DiscoveryDegree, GameManager.Instance.CurEnvironmentBag.ExploreCompleted));
    }

    protected override void Init()
    {
        exploreButton.onClick.RemoveAllListeners();
        exploreButton.onClick.AddListener(Explore);
    }

    /// <summary>
    /// 探索
    /// </summary>
    private void Explore()
    {
        var pos = envCardTransform.anchoredPosition;

        var seq = DOTween.Sequence();

        // 1. 牌堆抖动
        seq.Join(envCardTransform.DOShakePosition(pDuration, pStrength, vibrato: pVibrato, fadeOut: false)); // 位置抖动
        seq.Join(envCardTransform.DOShakeRotation(rDuration, rStrength, vibrato: rVibrato, fadeOut: false)); // 旋转抖动

        // 2. 抽牌
        seq.AppendCallback(() =>
        {
            GameManager.Instance.HandleExplore(out var tip, out var droppedCards);
            GameManager.Instance.AddCardsWithTween(droppedCards, envCardTransform.position, false);
            // 提示
            exploreButton.ShowTip(tip);
        });

        // 3. 归位
        seq.Append(envCardTransform.DOAnchorPos(pos, .1f));
        seq.Join(envCardTransform.DORotateQuaternion(Quaternion.identity, .1f));

        // 等待抽牌动画完成
        MouseManager.Instance.Wait(seq.Duration());
    }

    /// <summary>
    /// 移动到指定环境
    /// </summary>
    public override void DisplayBag(Bag bag)
    {
        base.DisplayBag(bag);

        var env = bag as EnvironmentBag;

        stateSliders.Clear();

        MonoUtility.DestroyAllChildren(stateLayout);

        // 压强都显示
        pressureLevel = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Controls/EnvironmentState/PressureLevel"), stateLayout).GetComponent<UIPressureLevel>();
        pressureLevel.SetValue(env.PressureLevel);

        // 是否铺设电缆都显示
        hasCabbleToggle = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Controls/EnvironmentState/HasCable"), stateLayout).GetComponent<UIStateToggle>();
        hasCabbleToggle.SetStateName("铺设电缆");
        hasCabbleToggle.SetValue(env.HasCable);

        // 铺设电缆才显示电力
        UIStateSlider slider;
        if (env.HasCable)
        {
            slider = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Controls/EnvironmentState/Electricity"), stateLayout).GetComponent<UIStateSlider>();
            slider.SetValue(StateManager.Instance.Electricity);
            stateSliders.Add(EnvironmentStateEnum.Electricity, slider);
        }

        // 在飞船内显示水平面高度
        if (env.PlaceData.isInSpacecraft)
        {
            slider = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Controls/EnvironmentState/WaterLevel"), stateLayout).GetComponent<UIStateSlider>();
            slider.SetValue(StateManager.Instance.WaterLevel);
            stateSliders.Add(EnvironmentStateEnum.WaterLevel, slider);
        }

        // 其他状态显示
        foreach (var (state, value) in env.StateDict)
        {
            slider = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Controls/EnvironmentState/" + state.ToString()), stateLayout).GetComponent<UIStateSlider>();
            slider.SetValue(value);
            stateSliders.Add(state, slider);
        }

        MonoUtility.UpdateLayoutSize(stateLayout.GetComponent<VerticalLayoutGroup>());

        // 显示环境名称
        placeNameText.text = $"{env.PlaceData.placeName}";

        // 探索事件
        DisplayDiscoveryDegree((env.DiscoveryDegree, env.ExploreCompleted));

        // 显示图片
        environmentImage.sprite = env.PlaceData.placeImage;
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

        var text = exploreButton.GetComponentInChildren<Text>();
        if (args.completed)
        {
            exploreButton.normalImage.gameObject.SetActive(false);
            exploreButton.Interactable = false;
            text.text = "探索完成";
            text.color = ColorManager.Cyan;

            // 不再显示探索提示
            hoveredTipController.enabled = false;
        }
        else if (args.degree == 1)
        {
            // 深入探索
            exploreButton.normalImage.gameObject.SetActive(true);
            exploreButton.Interactable = true;
            text.text = "深入探索";
            text.color = ColorManager.White;

            hoveredTipController.enabled = true;
        }
        else
        {
            exploreButton.normalImage.gameObject.SetActive(true);
            exploreButton.Interactable = true;
            text.text = "开始探索";
            text.color = ColorManager.White;

            hoveredTipController.enabled = true;
        }

        // 按钮是否能够交互
        exploreButton.Interactable = exploreButton.Interactable && GameManager.Instance.CanMoveExplore();
    }
}