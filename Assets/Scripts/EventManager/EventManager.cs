using System;
using System.Collections.Generic;
using UnityEngine;

// public void Start()
// {
//     EventManager.Instance.AddListener<BodyConfirmedEventArgs>(EventType.BodyConfirmed, OnBodyConfirmed);
// }
// public void OnDestroy()
// {
//     EventManager.Instance.RemoveListener<BodyConfirmedEventArgs>(EventType.BodyConfirmed, OnBodyConfirmed);
// }
//EventManager.Instance.TriggerEvent(EventType.BodyConfirmed,new BodyConfirmedEventArgs(1));

public class EventManager : MonoBehaviour
{
    private static EventManager instance;
    public static EventManager Instance
    { get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<EventManager>();
                if (instance == null)
                {
                    GameObject managerObj = new GameObject("EventManager");
                    instance = managerObj.AddComponent<EventManager>();
                    DontDestroyOnLoad(managerObj); // 跨场景保持实例
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        // 确保只有一个实例
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        

    }
    
    // 清理所有事件订阅
    private void CleanupEvents()
    {
        if (eventDictionary != null)
        {
            eventDictionary.Clear();
        }
    }
    
    // 确保在销毁时清理事件
    private void OnDestroy()
    {
        // 重要：不要在这里创建新对象!
        // 只执行清理操作
        if (instance == this)
        {
            CleanupEvents();
            instance = null;
        }
    }
    // 事件字典：无参数事件
    private Dictionary<EventType, Action> eventDictionary = new Dictionary<EventType, Action>();

    // 事件字典：带参数事件
    private Dictionary<EventType, Dictionary<Type, Delegate>> paramEventDictionary
        = new Dictionary<EventType, Dictionary<Type, Delegate>>();



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