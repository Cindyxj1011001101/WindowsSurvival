using System;
using System.Collections.Generic;

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

    private List<InGameEvent> inGameEvents = new();

    private Dictionary<string, float> eventsOnCooldown = new(); // 冷却中的事件字典，键为事件名称，值为剩余冷却时间

    private InGameEventManager() { }

    public void Init()
    {
        // 注册所有事件
        RegistInGameEvents();
        // 加载冷却中的事件
        LoadEventsOnCoolDown();
        // 监听结算事件
        UpdateManager.Instance.InGameEventUpdate.AddListener(Update);

        foreach (var evt in inGameEvents)
        {
            UnityEngine.Debug.Log($"Registered Event: {evt.eventName}, Threat Level: {evt.threatLevel}, " +
                $"Basic Trigger Weight: {evt.basicTriggerWeight}, Trigger Interval: {evt.triggerInterval}");
        }
    }

    private void Update()
    {
        UpdateEventCooldowns();
        TryTriggerEvent();
    }

    private void RegistInGameEvents()
    {
        var configs = ExcelReader.ReadInGameEventConfig("InGameEventConfig");
        foreach (var config in configs)
        {
            inGameEvents.Add(CreateEventInstance(config));
        }
    }

    private void LoadEventsOnCoolDown()
    {

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

    private void UpdateEventCooldowns()
    {
        var keys = new List<string>(eventsOnCooldown.Keys);
        foreach (var eventName in keys)
        {
            eventsOnCooldown[eventName] -= TimeManager.Instance.SettleInterval;
            if (eventsOnCooldown[eventName] <= 0)
            {
                eventsOnCooldown.Remove(eventName);
            }
        }
    }

    private void TryTriggerEvent()
    {

    }
}