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
    [SerializeField] private Slider targetCoordSlider;
    [SerializeField] private HoverableButton moveLeftButton;
    [SerializeField] private HoverableButton moveRightButton;
    [SerializeField] private HoverableButton executeMoveButton;
    [SerializeField] private Text targetPosition;
    [SerializeField] private Text currentPosition;
    [SerializeField] private Image fillBetween;

    private const float MoveDistResolution = .5f; // 移动距离分辨率

    private HoverTipController exploreTipController;
    private HoverTipController moveTipController;

    private EnvironmentBag CurEnv => GameManager.Instance.CurEnvironmentBag;
    private Player Player => GameManager.Instance.Player;
    private float TargetPosition => targetCoordSlider.value * MoveDistResolution;

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
                desc = "探索该地点" + desc;
                exploreTipController.SetTip(desc, time, playerEffects, null);
            }
            else
                exploreTipController.SetTip("身上太重了，无法探索");
        });

        // 移动消耗显示
        moveTipController = executeMoveButton.gameObject.AddComponent<HoverTipController>();
        moveTipController.onPointerEnter.AddListener(() =>
        {
            if (GameManager.Instance.CanMoveExplore())
            {
                var dist = Mathf.Abs(Player.Coordinate.Position - TargetPosition);
                var basicMoveTime = Mathf.CeilToInt(dist / Player.moveDistPerMin);
                var (desc, time, playerEffects) = GameManager.Instance.GetMoveEffects(basicMoveTime, CurEnv.PlaceData.placeType);
                desc = $"前往坐标 {TargetPosition:0.0} 处" + desc;
                moveTipController.SetTip(desc, time, playerEffects, null);
            }
            else
                moveTipController.SetTip("身上太重了，无法移动");
        });

        // 当前坐标显示
        currentCoordSlider.onValueChanged.AddListener((v) =>
        {
            currentPosition.text = (v * MoveDistResolution).ToString("0.0");
            FillBetween();    
        });

        // 选择距离
        targetCoordSlider.onValueChanged.AddListener((v) =>
        {
            targetPosition.text = (v * MoveDistResolution).ToString("0.0");
            FillBetween();
        });
        moveLeftButton.onClick.AddListener(() =>
        {
            targetCoordSlider.value--;
        });
        moveRightButton.onClick.AddListener(() =>
        {
            targetCoordSlider.value++;
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
        // 注册负重变化事件
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
    private void OnLoadChanged(PlayerStateEnum state)
    {
        if (state != PlayerStateEnum.Load) return;

        DisplayDiscoveryDegree((CurEnv.DiscoveryDegree, CurEnv.ExploreCompleted));
        executeMoveButton.Interactable = GameManager.Instance.CanMoveExplore();
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
        currentCoordSlider.maxValue = targetCoordSlider.maxValue = CurEnv.PlaceData.maxCoord / MoveDistResolution;
    }

    /// <summary>
    /// 用白色填充 current coord slider handle 和 target corrd slider handle 之间的部分
    /// </summary>
    private void FillBetween()
    {
        fillBetween.transform.position = (currentCoordSlider.handleRect.position + targetCoordSlider.handleRect.position) / 2;
        fillBetween.rectTransform.sizeDelta = new(Mathf.Abs(currentCoordSlider.handleRect.position.x - targetCoordSlider.handleRect.position.x), fillBetween.rectTransform.sizeDelta.y);
    }

    /// <summary>
    /// 显示玩家位置
    /// </summary>
    private void DisplayPlayerPosition()
    {
        currentCoordSlider.value = targetCoordSlider.value = Player.Coordinate.Position / MoveDistResolution;

        currentPosition.text = targetPosition.text = Player.Coordinate.Position.ToString("0.0");
        FillBetween();
    }

    /// <summary>
    /// 执行移动
    /// </summary>
    private void ExecuteMove()
    {
        if (Mathf.Abs(TargetPosition - Player.Coordinate.Position) < MoveDistResolution) return;

        // TODO：移动方法
        GameManager.Instance.Move(TargetPosition);
        DisplayPlayerPosition();
    }
}