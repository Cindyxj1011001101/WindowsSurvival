using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class Reduce
{
    public int maxReduceCount;
    public int curReduceCount;
    public float reduceRate;

    [JsonIgnore]
    public float ReduceRate => Mathf.Pow(reduceRate, curReduceCount);

    public Reduce() { }

    public Reduce(int maxReduceCount, float reduceRate)
    {
        this.curReduceCount = 0;
        this.maxReduceCount = maxReduceCount;
        this.reduceRate = reduceRate;
    }

    public void AddReduceCount()
    {
        curReduceCount++;
        curReduceCount = Mathf.Clamp(curReduceCount, 0, maxReduceCount);
    }
}

public class GlobalData
{
    #region 每日衰减
    public Dictionary<string, Reduce> reduceActionDict = new();

    public void AddReduceCount(string key)
    {
        if (reduceActionDict.TryGetValue(key, out var value))
        {
            value.AddReduceCount();
        }
    }

    public float GetReduceRate(string key)
    {
        if (reduceActionDict.ContainsKey(key))
        {
            return reduceActionDict[key].ReduceRate;
        }
        return 1;
    }

    public int GetCurrentReduceCount(string key)
    {
        if (reduceActionDict.ContainsKey(key))
        {
            return reduceActionDict[key].curReduceCount;
        }
        return -1;
    }

    public void AddReduceAction(string key, Reduce reduce)
    {
        if (!reduceActionDict.ContainsKey(key))
            reduceActionDict.Add(key, reduce);
    }

    public bool IsReduceCountMax(string key)
    {
        if (!reduceActionDict.TryGetValue(key, out var value)) return false;

        return value.curReduceCount == value.maxReduceCount;
    }
    #endregion
}