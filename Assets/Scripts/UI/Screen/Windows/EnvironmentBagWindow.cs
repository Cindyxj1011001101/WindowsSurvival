using DG.Tweening;
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
    [SerializeField] private UIStateSlider discoveryDegreeSlider; // 探索度显示
    [SerializeField] private Text placeNameText; // 环境名称
    [SerializeField] private Image environmentImage; // 环境图片
    [SerializeField] private HoverableButton exploreButton; // 探索按钮
    [SerializeField] private RectTransform stateLayout;
    [SerializeField] private RectTransform envCardTransform;

    [SerializeField] private UIStateToggle hasCabble; // 是否铺设电缆
    [SerializeField] private UIPressureLevel pressureLevel; // 压强等级
    [HideInInspector] public Dictionary<EnvironmentStateEnum, UIStateSlider> continuousValueStates = new(); // 环境状态显示

    [SerializeField] private Slider currentCoordSlider;
    [SerializeField] private Slider nextCoordSlider;
    [SerializeField] private HoverableButton moveLeftButton;
    [SerializeField] private HoverableButton moveRightButton;
    [SerializeField] private HoverableButton executeMoveButton;
    [SerializeField] private Text nextPosition;
    [SerializeField] private Text currentPosition;
    [SerializeField] private Image fillBetween;

    private const float MoveDistResolution = .5f; // 移动距离分辨率

    private HoverTipController exploreTipController;

    private EnvironmentBag CurEnv => GameManager.Instance.CurEnvironmentBag;
    private Player Player => GameManager.Instance.Player;
    private float NextPosition => nextCoordSlider.value * MoveDistResolution;

    protected override void Awake()
    {
        base.Awake();

        // 探索按钮事件
        exploreButton.onClick.AddListener(Explore);

        // 探索消耗显示
        exploreTipController = exploreButton.gameObject.AddComponent<HoverTipController>();
        exploreTipController.onPointerEnter.AddListener(() =>
        {
            if (GameManager.Instance.CanMoveExplore())
            {
                var (desc, time, playerEffects) = GameManager.Instance.GetExploreEffects();
                exploreTipController.SetTip(desc, time, playerEffects, null);
            }
            else
                exploreTipController.SetTip("身上太重了，无法探索");
        });

        // 移动

        // 当前坐标显示
        currentCoordSlider.onValueChanged.AddListener((v) =>
        {
            currentPosition.text = (v * MoveDistResolution).ToString();
            FillBetween();    
        });

        // 选择距离
        nextCoordSlider.onValueChanged.AddListener((v) =>
        {
            nextPosition.text = (v * MoveDistResolution).ToString();
            FillBetween();
        });
        moveLeftButton.onClick.AddListener(() =>
        {
            nextCoordSlider.value--;
        });
        moveRightButton.onClick.AddListener(() =>
        {
            nextCoordSlider.value++;
        });

        // 执行移动
        executeMoveButton.onClick.AddListener(ExecuteMove);

        // 注册状态
        foreach (Transform c in stateLayout)
        {
            if (c.TryGetComponent<UIStateSlider>(out var s))
                continuousValueStates.Add((EnvironmentStateEnum)Enum.Parse(typeof(EnvironmentStateEnum), c.name), s);
        }

        // 注册探索度变化事件
        EventManager.Instance.AddListener<(float, bool)>(EventType.ChangeDiscoveryDegree, DisplayDiscoveryDegree);
        // 注册环境移动事件
        EventManager.Instance.AddListener<EnvironmentBag>(EventType.ChangeEnv, DisplayBag);
        // 注册环境状态变化事件
        EventManager.Instance.AddListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnEnvironmentStateChanged);
        // 玩家背包卡牌变化
        EventManager.Instance.AddListener<PlayerStateEnum>(EventType.RefreshPlayerState, OnLoadChanged);
    }

    private void OnDestroy()
    {
        // 移除事件
        EventManager.Instance.RemoveListener<(float, bool)>(EventType.ChangeDiscoveryDegree, DisplayDiscoveryDegree);
        EventManager.Instance.RemoveListener<EnvironmentBag>(EventType.ChangeEnv, DisplayBag);
        EventManager.Instance.RemoveListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnEnvironmentStateChanged);
        EventManager.Instance.RemoveListener<PlayerStateEnum>(EventType.RefreshPlayerState, OnLoadChanged);
    }

    /// <summary>
    /// 监听负重变化，负重变化会导致探索按钮的提示变化
    /// </summary>
    /// <param name="args"></param>
    private void OnLoadChanged(PlayerStateEnum state)
    {
        if (state != PlayerStateEnum.Load) return;

        DisplayDiscoveryDegree((CurEnv.DiscoveryDegree, CurEnv.ExploreCompleted));
    }

    protected override void Init()
    {
    }

    /// <summary>
    /// 探索
    /// </summary>
    private void Explore()
    {
        var seq = envCardTransform.PunchAndBounce(() =>
        {
            GameManager.Instance.HandleExplore(out var tip, out var droppedCards);
            GameManager.Instance.AddCardsWithTween(droppedCards, false, envCardTransform.position);
            // 提示
            exploreButton.ShowTip(tip);
        });

        // 等待抽牌动画完成
        MouseManager.Instance.Wait(seq.Duration());
    }

    public override void DisplayBag(Bag bag)
    {
        base.DisplayBag(bag);

        var env = bag as EnvironmentBag;

        foreach (var s in continuousValueStates.Values)
        {
            s.gameObject.SetActive(false);
        }

        // 压强都显示
        pressureLevel.SetValue(env.PressureLevel);

        // 是否铺设电缆都显示
        hasCabble.SetValue(env.HasCable);

        // 铺设电缆才显示电力
        if (env.HasCable)
        {
            continuousValueStates[EnvironmentStateEnum.Electricity].gameObject.SetActive(true);
            continuousValueStates[EnvironmentStateEnum.Electricity].SetValue(StateManager.Instance.Electricity);
        }

        // 在飞船内显示水平面高度
        if (env.PlaceData.isInSpacecraft)
        {
            continuousValueStates[EnvironmentStateEnum.WaterLevel].gameObject.SetActive(true);
            continuousValueStates[EnvironmentStateEnum.WaterLevel].SetValue(StateManager.Instance.WaterLevel);
        }

        // 其他状态显示
        foreach (var (state, value) in env.StateDict)
        {
            continuousValueStates[state].gameObject.SetActive(true);
            continuousValueStates[state].SetValue(value);
        }

        MonoUtility.UpdateLayoutSize(stateLayout.GetComponent<VerticalLayoutGroup>());

        // 显示环境名称
        placeNameText.text = $"{env.PlaceData.placeName}";

        // 探索事件
        DisplayDiscoveryDegree((env.DiscoveryDegree, env.ExploreCompleted));

        // 显示图片
        environmentImage.sprite = env.PlaceData.placeImage;
        environmentImage.SetNativeSize();

        // 显示坐标系
        DisplayCoordinateSystem();

        // 显示玩家位置
        DisplayPlayerPosition();
    }

    /// <summary>
    /// 单个环境状态变化UI刷新
    /// </summary>
    private void OnEnvironmentStateChanged(RefreshEnvironmentStateArgs args)
    {
        // 不是当前地点的值变化不显示
        if (args.place != CurEnv.PlaceData.placeType) return;

        switch (args.stateEnum)
        {
            case EnvironmentStateEnum.HasCable:
                hasCabble.SetValue(args.hasCable);
                break;
            case EnvironmentStateEnum.PressureLevel:
                pressureLevel.SetValue(args.pressureLevel);
                break;
            default:
                // 不存在这个状态不显示
                if (!continuousValueStates.ContainsKey(args.stateEnum)) return;
                continuousValueStates[args.stateEnum].GetComponent<UIStateSlider>().SetValue(args.stateValue);
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
            exploreTipController.enabled = false;
        }
        else if (args.degree == 1)
        {
            // 深入探索
            exploreButton.normalImage.gameObject.SetActive(true);
            exploreButton.Interactable = true;
            text.text = "深入探索";
            text.color = ColorManager.White;

            exploreTipController.enabled = true;
        }
        else
        {
            exploreButton.normalImage.gameObject.SetActive(true);
            exploreButton.Interactable = true;
            text.text = "开始探索";
            text.color = ColorManager.White;

            exploreTipController.enabled = true;
        }

        // 按钮是否能够交互
        exploreButton.Interactable = exploreButton.Interactable && GameManager.Instance.CanMoveExplore();
    }

    /// <summary>
    /// 显示地点坐标系
    /// </summary>
    private void DisplayCoordinateSystem()
    {
        currentCoordSlider.maxValue = nextCoordSlider.maxValue = CurEnv.PlaceData.maxCoord / MoveDistResolution;
    }

    /// <summary>
    /// 用白色填充 current coord slider handle 和 next corrd slider handle 之间的部分
    /// </summary>
    private void FillBetween()
    {
        fillBetween.transform.position = (currentCoordSlider.handleRect.position + nextCoordSlider.handleRect.position) / 2;
        fillBetween.rectTransform.sizeDelta = new(Mathf.Abs(currentCoordSlider.handleRect.position.x - nextCoordSlider.handleRect.position.x), fillBetween.rectTransform.sizeDelta.y);
    }

    /// <summary>
    /// 显示玩家位置
    /// </summary>
    private void DisplayPlayerPosition()
    {
        currentCoordSlider.value = nextCoordSlider.value = Player.Coordinate.Position / MoveDistResolution;

        currentPosition.text = nextPosition.text = Player.Coordinate.Position.ToString();
        FillBetween();
    }

    /// <summary>
    /// 执行移动
    /// </summary>
    private void ExecuteMove()
    {
        if (Mathf.Abs(NextPosition - Player.Coordinate.Position) < MoveDistResolution) return;

        Player.Coordinate.SetPosition(NextPosition);
        DisplayPlayerPosition();
    }
}