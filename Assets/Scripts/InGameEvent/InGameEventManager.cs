using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InGameEventManager
{
    public static InGameEventManager Instance { get; } = new InGameEventManager();

    private Dictionary<string, Type> eventNameTypeDict = new()
    {
        { "入侵", typeof(Invasion) },
        { "恒星耀斑", typeof(StellarFlare) },
        { "生物迁徙经过", typeof(BiologicalMigration) },
        { "出现裂缝", typeof(CracksAppear) },
        { "流星坠落", typeof(MeteorFall) },
        { "鼠患", typeof(RatInfestation) },
        { "灵光乍现", typeof(InspirationFlash) },
        { "呕吐", typeof(Vomit) },
    };

    private List<InGameEvent> allEvents = new();

    public Dictionary<string, float> EventsOnCooldown { get; private set; } = new(); // 冷却中的事件字典，键为事件名称，值为剩余冷却时间

    private float sensitivity = 0.3f; // 敏感系数
    public float TrendValue { get; private set; } = 0; // 趋势值
    
    // 趋势值边界
    private const float MAX_TREND_VALUE = 4f;
    private const float MIN_TREND_VALUE = -4f;

    private const float EVENT_TRIGGER_PROB = 1 / 96f; // 每次结算触发事件的基础概率（约1.04%）

    private InGameEventManager()
    {
        // 注册所有事件
        RegistInGameEvents();
        // 读取存档数据
        LoadData();
    }

    public void Init()
    {
        // 监听结算事件
        UpdateManager.Instance.InGameEventUpdate.AddListener(Update);
    }

    #region 初始化
    private void RegistInGameEvents()
    {
        var configs = ExcelReader.ReadInGameEventConfig("InGameEventConfig");
        foreach (var config in configs)
        {
            allEvents.Add(CreateEventInstance(config));
        }
    }

    private void LoadData()
    {
        var data = GameDataManager.Instance.InGameEventData;

        // 读取趋势值
        TrendValue = data.trendValue;
        // 读取冷却中的事件
        EventsOnCooldown = data.eventsOnCooldown;
    }

    private InGameEvent CreateEventInstance(InGameEventConfig config)
    {
        if (eventNameTypeDict.TryGetValue(config.EventName, out Type eventType))
        {
            var instance = (InGameEvent)Activator.CreateInstance(eventType);
            instance.eventName = config.EventName;
            instance.threatLevel = config.ThreatLevel;
            instance.basicTriggerWeight = config.BasicTriggerWeight;
            instance.triggerInterval = config.TriggerInterval;
            return instance;
        }
        else
        {
            throw new ArgumentException($"未知的事件名称: {config.EventName}");
        }
    }
    #endregion

    private void Update()
    {
        UpdateEventCooldowns();
        TryTriggerEvent();
    }

    private void UpdateEventCooldowns()
    {
        var keys = new List<string>(EventsOnCooldown.Keys);
        foreach (var eventName in keys)
        {
            EventsOnCooldown[eventName] -= TimeManager.SETTLEMENT_INTERVAL;
            if (EventsOnCooldown[eventName] <= 0)
            {
                EventsOnCooldown.Remove(eventName);
            }
        }
    }

    /// <summary>
    /// 尝试触发事件
    /// </summary>
    public void TryTriggerEvent()
    {
        // 首先根据基础概率决定是否尝试触发事件
        if (UnityEngine.Random.value > EVENT_TRIGGER_PROB) return;

        Debug.Log("尝试触发事件");

        // 获取可触发的事件候选列表（检查每个事件的独立冷却）
        var candidateEvents = GetCandidateEvents();

        foreach (var e in candidateEvents)
        {
            Debug.Log($"候选事件: {e.eventName}");
        }

        if (candidateEvents.IsNullOrEmpty()) return;

        // 计算每个事件的触发权重
        var eventWeights = CalculateEventWeights(candidateEvents);

        // 根据权重随机选择事件
        var selectedEvent = SelectEventByWeight(eventWeights);

        if (selectedEvent == null) return;

        Debug.Log($"触发事件: {selectedEvent.eventName}");

        // 更新该事件冷却时间
        EventsOnCooldown.Add(selectedEvent.eventName, selectedEvent.TriggerIntervalMinutes);

        // 更新趋势值
        UpdateTrendValue(selectedEvent.threatLevel);

        // 触发事件
        //selectedEvent.TriggerThisEvent();
    }

    /// <summary>
    /// 获取可触发的事件列表（检查每个事件的独立冷却时间）
    /// </summary>
    private List<InGameEvent> GetCandidateEvents()
    {
        return allEvents.Where(e => e.CanTriggerThisEvent() && IsEventReady(e)).ToList();
    }

    /// <summary>
    /// 检查事件是否已冷却完成
    /// </summary>
    private bool IsEventReady(InGameEvent gameEvent)
    {
        return !EventsOnCooldown.ContainsKey(gameEvent.eventName);
    }

    /// <summary>
    /// 计算所有候选事件的触发权重
    /// </summary>
    private Dictionary<InGameEvent, float> CalculateEventWeights(List<InGameEvent> candidates)
    {
        var weights = new Dictionary<InGameEvent, float>();

        // 首先计算每个事件的分子部分
        var numerators = new Dictionary<InGameEvent, float>();
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
    private InGameEvent SelectEventByWeight(Dictionary<InGameEvent, float> eventWeights)
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