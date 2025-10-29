using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 游戏事件管理器
/// </summary>
public class GameEventManager : IManager
{
    public static GameEventManager Instance { get; } = new GameEventManager();

    private List<GameEvent> eventTemplates = new();

    public Dictionary<string, GameEvent> OngoingEvents { get; private set; } = new(); // 进行中的事件列表

    public Dictionary<string, float> EventsOnCooldown { get; private set; } = new(); // 冷却中的事件字典，键为事件名称，值为剩余冷却时间

    private float sensitivity = 0.3f; // 敏感系数
    public float TrendValue { get; private set; } = 0; // 趋势值
    
    // 趋势值边界
    private const float MAX_TREND_VALUE = 4f;
    private const float MIN_TREND_VALUE = -4f;

    private const float EVENT_TRIGGER_PROB = 1f; // 每次结算触发事件的基础概率（约1.04%），期望触发间隔为24小时

    public InvasionEventConfig InvasionEventConfig { get; private set; }

    private GameEventManager()
    {
        // 注册所有事件
        eventTemplates = ExcelReader.ReadGameEventConfig("GameEventConfig");
    }

    #region 初始化
    public void Init()
    {
        // 读取存档数据
        LoadData();
        // 监听结算事件
        UpdateManager.Instance.InGameEventUpdate.AddListener(Update);
    }

    public void Reset()
    {
        OngoingEvents = new();
        EventsOnCooldown = new();
        InvasionEventConfig = new();
        UpdateManager.Instance.InGameEventUpdate.RemoveListener(Update);
    }

    private void LoadData()
    {
        var data = GameDataManager.Instance.GameEventData;
        // 读取趋势值
        TrendValue = data.trendValue;
        // 读取冷却中的事件
        EventsOnCooldown = data.eventsOnCooldown;
        // 读取持续中的事件
        OngoingEvents = data.ongoingEvents;
        // 读取入侵事件配置
        InvasionEventConfig = data.invasionConfig;
    }
    #endregion

    private void Update()
    {
        UpdateEventCooldowns();
        UpdateOngoingEvents();
        TryTriggerEvent();
    }

    private void UpdateEventCooldowns()
    {
        var keys = new List<string>(EventsOnCooldown.Keys);
        foreach (var eventTypeName in keys)
        {
            EventsOnCooldown[eventTypeName] -= TimeManager.SETTLEMENT_INTERVAL;
            if (EventsOnCooldown[eventTypeName] <= 0)
            {
                EventsOnCooldown.Remove(eventTypeName);
            }
        }
    }

    private void UpdateOngoingEvents()
    {
        var keys = new List<string>(OngoingEvents.Keys);
        foreach (var eventTypeName in keys)
        {
            var gameEvent = OngoingEvents[eventTypeName];
            gameEvent.OnUpdate();
            if (gameEvent.IsEventEnded())
            {
                CancelGameEvent(gameEvent);
            }
        }
    }

    private void CancelGameEvent(GameEvent gameEvent)
    {
        OngoingEvents.Remove(gameEvent.GetType().Name);
        gameEvent.OnEnd();
        EventManager.Instance.TriggerEvent(EventType.OnGameEventEnd, gameEvent);
        Debug.Log($"事件结束：{gameEvent.eventName}");
    }

    /// <summary>
    /// 尝试触发事件
    /// </summary>
    private void TryTriggerEvent()
    {
        // 首先根据基础概率决定是否尝试触发事件
        if (UnityEngine.Random.value > EVENT_TRIGGER_PROB) return;

        Debug.Log("尝试触发事件");

        // 获取可触发的事件候选列表（检查每个事件的独立冷却）
        var candidateEvents = GetCandidateEvents();
        if (candidateEvents.IsNullOrEmpty()) return;

        // 计算每个事件的触发权重
        var eventWeights = CalculateEventWeights(candidateEvents);

        // 根据权重随机选择事件
        var selectedEventTemplete = SelectEventByWeight(eventWeights);
        // 深拷贝实例
        var selectedEvent = JsonManager.DeepCopy(selectedEventTemplete);

        if (selectedEvent == null) return;

        TriggerGameEvent(selectedEvent);
    }

    private void TriggerGameEvent(GameEvent gameEvent)
    {
        var eventTypeName = gameEvent.GetType().Name;

        // 更新趋势值
        UpdateTrendValue(gameEvent.threatLevel);

        // 从事件触发时开始计算冷却时间
        EventsOnCooldown.Add(eventTypeName, gameEvent.CoolDown);

        // 事件触发逻辑
        gameEvent.OnTrigger();

        // 对于持续性事件，添加到持续事件列表
        if (gameEvent.remainingMinutes > 0)
            OngoingEvents.Add(eventTypeName, gameEvent);

        EventManager.Instance.TriggerEvent(EventType.OnGameEventTrigger, gameEvent);
        Debug.Log($"触发事件：{gameEvent.eventName}，持续时间：{gameEvent.remainingMinutes}分钟");
    }

    /// <summary>
    /// 获取可触发的事件列表（检查每个事件的独立冷却时间）
    /// </summary>
    private List<GameEvent> GetCandidateEvents()
    {
        return eventTemplates.Where(e => e.CanTriggerThisEvent() && IsEventReady(e)).ToList();
    }

    public bool IsEventOngoing<T>() where T : GameEvent
    {
        return OngoingEvents.ContainsKey(typeof(T).Name);
    }

    /// <summary>
    /// 检查事件是否不在持续并且冷却完成
    /// </summary>
    private bool IsEventReady(GameEvent gameEvent)
    {
        var eventTypeName = gameEvent.GetType().Name;
        return !EventsOnCooldown.ContainsKey(eventTypeName) && !OngoingEvents.ContainsKey(eventTypeName);
    }

    /// <summary>
    /// 计算所有候选事件的触发权重
    /// </summary>
    private Dictionary<GameEvent, float> CalculateEventWeights(List<GameEvent> candidates)
    {
        var weights = new Dictionary<GameEvent, float>();

        // 首先计算每个事件的分子部分
        var numerators = new Dictionary<GameEvent, float>();
        float denominator = 0f;

        foreach (var gameEvent in candidates)
        {
            // 计算公式分子：基础触发权重 × e^(敏感系数 × 趋势值 × 威胁程度)
            float exponent = sensitivity * TrendValue * gameEvent.threatLevel;
            float numerator = gameEvent.basicTriggerWeight * Mathf.Exp(exponent);

            numerators[gameEvent] = numerator;
            denominator += numerator;
        }

        // 计算最终权重
        foreach ((var e, var numerator) in numerators)
        {
            weights.Add(e, denominator > 0 ? numerator / denominator : 0f);
        }

        return weights;
    }

    /// <summary>
    /// 根据权重随机选择事件
    /// </summary>
    private GameEvent SelectEventByWeight(Dictionary<GameEvent, float> eventWeights)
    {
        // 计算总权重
        float totalWeight = eventWeights.Values.Sum();

        // 随机选择
        float randomValue = UnityEngine.Random.value * totalWeight;
        float currentSum = 0f;

        foreach ((var e, var weight) in eventWeights)
        {
            currentSum += weight;
            if (randomValue <= currentSum)
                return e;
        }

        return eventWeights.Keys.Last();
    }

    /// <summary>
    /// 更新趋势值
    /// </summary>
    private void UpdateTrendValue(int threatLevel)
    {
        TrendValue -= threatLevel;
        TrendValue = Math.Clamp(TrendValue, MIN_TREND_VALUE, MAX_TREND_VALUE);
    }
}