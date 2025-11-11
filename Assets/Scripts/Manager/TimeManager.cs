using System;
using UnityEngine;

public class TimeManager : IManager
{
    public static TimeManager Instance { get; } = new();

    public const int SETTLEMENT_INTERVAL = 15; // 结算间隔
    public DateTime StartDateTime { get; private set; } = new(2020, 1, 1, 0, 0, 0);
    public DateTime CurTime { get; private set; }
    public int CurInterval { get; private set; }

    private DateTime lastDay;

    public int Day => (CurTime - StartDateTime).Days + 1;

    private bool timePassStopped = false;   // 时间流逝停止
    private bool timePassPaused = false;    // 时间流逝暂停
    private float timePauseEndTime = 0;     // 时间流逝暂停结束时间
    private int minutesToPass = 0;          // 待流逝的时间

    public void Init()
    {
        var timeData = GameDataManager.Instance.TimeData;

        if (!timeData.init)
        {
            //默认初始化
            CurTime = StartDateTime;
            CurInterval = SETTLEMENT_INTERVAL;
        }
        else
        {
            //从存档初始化
            CurTime = GameDataManager.Instance.TimeData.curTime;
            CurInterval = GameDataManager.Instance.TimeData.curIntervel;
        }

        // 初始化lastDay
        lastDay = CurTime.Date;

        // 处理帧更新事件
        PublicMono.Instance.AddUpdateListener(Update);
    }

    public void Reset()
    {
        PublicMono.Instance.RemoveUpdateListener(Update);
    }

    private void Update()
    {
        if (timePassPaused && Time.time >= timePauseEndTime)
        {
            // 继续时间流逝
            AddTime(minutesToPass);
        }
    }

    public void AddTime(int minutes)
    {
        timePassStopped = false;
        timePassPaused = false;
        minutesToPass = 0;

        EventManager.Instance.TriggerEvent(EventType.StartChangeTime);

        // 等待动画
        MouseManager.Instance.Wait();

        int timespan = minutes;

        // 以一分钟为粒度流逝时间
        while (!timePassStopped && timespan > 0)
        {
            timespan--;
            UpdateCurInterval();

            // 时间流逝暂停
            if (timePassPaused)
            {
                if (timespan > 0)
                    // 记录下待流逝的时间
                    minutesToPass = timespan;
                else
                    timePassPaused = false;
                // 退出循环
                break;
            }
        }

        EventManager.Instance.TriggerEvent(EventType.EndChangeTime);
    }

    private void UpdateCurInterval()
    {
        CurInterval--;
        CurTime = CurTime.AddMinutes(1);
        HandleAnotherDay();
        if (CurInterval <= 0)
        {
            // 每15分钟触发一次Update事件
            CurInterval = SETTLEMENT_INTERVAL;
            EventManager.Instance.TriggerEvent(EventType.Update);
        }
        EventManager.Instance.TriggerEvent(EventType.FineUpdate);
    }

    private bool HandleAnotherDay()
    {
        if (CurTime.Date == lastDay) return false;

        lastDay = CurTime.Date;
        EventManager.Instance.TriggerEvent(EventType.AnotherDay);
        return true;
    }

    /// <summary>
    /// 停止时间流逝
    /// </summary>
    public void StopTimePass()
    {
        timePassStopped = true;
    }

    /// <summary>
    /// 暂停时间流逝
    /// </summary>
    public void PauseTimePass(float duration)
    {
        timePassPaused = true;
        timePauseEndTime = Mathf.Max(timePauseEndTime, Time.time + duration);
    }
}