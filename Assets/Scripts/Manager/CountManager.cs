using System.Collections.Generic;
using UnityEngine;

public class CountManager : IManager
{
    public static CountManager Instance { get; } = new();

    private Dictionary<string, int> counts = new Dictionary<string, int>();

    public Dictionary<string, int> Counts => counts;

    private CountManager() { }

    public void Init()
    {
        // 从 GameDataManager 加载计数数据
        var countData = GameDataManager.Instance.CountData;
        if (countData != null && countData.init)
        {
            counts = new Dictionary<string, int>(countData.counts);
        }
        else
        {
            counts = new Dictionary<string, int>();
        }
    }

    public void Reset()
    {
        counts.Clear();
    }

    /// <summary>
    /// 改变计数值（增加或减少）
    /// </summary>
    /// <param name="countName">计数名</param>
    /// <param name="delta">变化量（正数为增加，负数为减少）</param>
    public void ChangeCount(string countName, int delta)
    {
        if (counts.ContainsKey(countName))
        {
            counts[countName] += delta;
        }
        else
        {
            counts[countName] = delta;
        }
        // 触发计数变化事件
        EventManager.Instance.TriggerEvent(EventType.CountChanged);
    }

    /// <summary>
    /// 设置计数值
    /// </summary>
    /// <param name="countName">计数名</param>
    /// <param name="value">要设置的值</param>
    public void SetCount(string countName, int value)
    {
        counts[countName] = value;
        // 触发计数变化事件
        EventManager.Instance.TriggerEvent(EventType.CountChanged);
    }

    /// <summary>
    /// 获取计数值
    /// </summary>
    /// <param name="countName">计数名</param>
    /// <returns>计数值，如果不存在则返回0</returns>
    public int GetCount(string countName)
    {
        if (counts.TryGetValue(countName, out int value))
        {
            return value;
        }
        return 0;
    }

    /// <summary>
    /// 获取所有计数的字典（用于调试和查看）
    /// </summary>
    /// <returns>所有计数的字典副本</returns>
    public Dictionary<string, int> GetAllCounts()
    {
        return new Dictionary<string, int>(counts);
    }

    /// <summary>
    /// 打印所有计数到控制台（用于调试）
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void PrintAllCounts()
    {
        UnityEngine.Debug.Log("=== 所有计数值 ===");
        if (counts.Count == 0)
        {
            UnityEngine.Debug.Log("当前没有任何计数。");
        }
        else
        {
            foreach (var kvp in counts)
            {
                UnityEngine.Debug.Log($"{kvp.Key} = {kvp.Value}");
            }
        }
        UnityEngine.Debug.Log("==================");
    }
}
