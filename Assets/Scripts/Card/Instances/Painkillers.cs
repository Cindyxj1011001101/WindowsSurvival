using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Painkillers : Card
{
    public int maxReduceCount;
    public int curReduceCount;
    public float ReduceRate;
    private Painkillers()
    {
        maxReduceCount = 2;
        curReduceCount = 0;
        ReduceRate = 0.5f;
        Events = new()
        {
            new Event("使用", "使用", Event_Use, null)
        };
    }
    public void Event_Use(out string tip) 
    {
        tip = string.Empty;
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.PainLevel, 50* Mathf.Pow(ReduceRate, curReduceCount));
        TimeManager.Instance.AddTime(5);
        curReduceCount++;
        if (curReduceCount >= maxReduceCount) curReduceCount = maxReduceCount;

    }
    protected override Action OnUpdate => () =>
    {
        if (TimeManager.Instance.AnotherDay()) curReduceCount = 0;  
    };
}