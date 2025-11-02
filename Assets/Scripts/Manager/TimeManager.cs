using System;

public class TimeManager : IManager
{
    public static TimeManager Instance { get; } = new();

    public const int SETTLEMENT_INTERVAL = 15; // 结算间隔
    public DateTime StartDateTime { get; private set; } = new(2020, 1, 1, 0, 0, 0);
    public DateTime CurTime { get; private set; }
    public int CurInterval { get; private set; }

    private DateTime lastDay;

    public int Day => (CurTime - StartDateTime).Days + 1;

    private bool timePassStopped = false;

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
    }

    public void Reset() { }

    public void AddTime(int minutes)
    {
        timePassStopped = false;
        EventManager.Instance.TriggerEvent(EventType.StartChangeTime);

        // 等待动画
        MouseManager.Instance.Wait();

        int timespan = minutes;

        while (!timePassStopped && timespan > 0)
        {
            timespan--;
            UpdateCurInterval();
        }

        EventManager.Instance.TriggerEvent(EventType.EndChangeTime);
    }

    private void UpdateCurInterval()
    {
        CurInterval--;
        EventManager.Instance.TriggerEvent(EventType.AddOneMinute);
        if (CurInterval <= 0)
        {
            CurInterval = SETTLEMENT_INTERVAL;
            EventManager.Instance.TriggerEvent(EventType.Update);
        }
        CurTime = CurTime.AddMinutes(1);
        HandleAnotherDay();
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
}