using System;
using System.Collections.Generic;

// public void Start()
// {
//     EventManager.Instance.AddListener<BodyConfirmedEventArgs>(EventType.BodyConfirmed, OnBodyConfirmed);
// }
// public void OnDestroy()
// {
//     EventManager.Instance.RemoveListener<BodyConfirmedEventArgs>(EventType.BodyConfirmed, OnBodyConfirmed);
// }
//EventManager.Instance.TriggerEvent(EventType.BodyConfirmed,new BodyConfirmedEventArgs(1));

public class EventManager
{
    public static EventManager Instance { get; } = new();

    // 事件字典：无参数事件
    private Dictionary<EventType, Action> eventDictionary = new();

    // 事件字典：带参数事件
    private Dictionary<EventType, Dictionary<Type, Delegate>> paramEventDictionary = new();

    #region 无参数事件
    // 添加监听
    public void AddListener(EventType eventType, Action listener)
    {
        if (!eventDictionary.ContainsKey(eventType))
        {
            eventDictionary.Add(eventType, null);
        }
        eventDictionary[eventType] += listener;
    }

    // 移除监听
    public void RemoveListener(EventType eventType, Action listener)
    {
        if (eventDictionary.ContainsKey(eventType))
        {
            eventDictionary[eventType] -= listener;
        }
    }

    // 触发事件
    public void TriggerEvent(EventType eventType)
    {
        if (eventDictionary.ContainsKey(eventType))
        {
            eventDictionary[eventType]?.Invoke();
        }
    }
    #endregion

    #region 带参数事件
    // 添加带参数的监听
    public void AddListener<T>(EventType eventType, Action<T> listener)
    {
        if (!paramEventDictionary.ContainsKey(eventType))
        {
            paramEventDictionary.Add(eventType, new Dictionary<Type, Delegate>());
        }

        var type = typeof(T);
        if (!paramEventDictionary[eventType].ContainsKey(type))
        {
            paramEventDictionary[eventType].Add(type, null);
        }

        paramEventDictionary[eventType][type]
            = Delegate.Combine(paramEventDictionary[eventType][type], listener);
    }

    // 移除带参数的监听
    public void RemoveListener<T>(EventType eventType, Action<T> listener)
    {
        if (paramEventDictionary.ContainsKey(eventType))
        {
            var type = typeof(T);
            if (paramEventDictionary[eventType].ContainsKey(type))
            {
                paramEventDictionary[eventType][type]
                    = Delegate.Remove(paramEventDictionary[eventType][type], listener);
            }
        }
    }

    // 触发带参数的事件
    public void TriggerEvent<T>(EventType eventType, T parameter)
    {
        if (paramEventDictionary.ContainsKey(eventType))
        {
            var type = typeof(T);
            if (paramEventDictionary[eventType].ContainsKey(type))
            {
                var action = paramEventDictionary[eventType][type] as Action<T>;
                action?.Invoke(parameter);
            }
        }
    }
    #endregion

    // 清除所有事件
    public void ClearEvents()
    {
        eventDictionary.Clear();
        paramEventDictionary.Clear();
    }
}