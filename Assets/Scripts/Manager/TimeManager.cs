using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public const int SETTLEMENT_INTERVAL = 15; // 结算间隔
    public DateTime StartDateTime { get; private set; } = new(2020, 1, 1, 0, 0, 0);
    public DateTime CurTime { get; private set; }
    public int CurInterval { get; private set; }

    private DateTime lastDay;

    public int Day => (CurTime - StartDateTime).Days + 1;

    private static TimeManager instance;
    public static TimeManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TimeManager>();
                if (instance == null)
                {
                    GameObject managerObj = new GameObject("TimeManager");
                    instance = managerObj.AddComponent<TimeManager>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        Init();
    }

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

    public void AddTime(int minute)
    {
        EventManager.Instance.TriggerEvent(EventType.StartChangeTime);

        // 等待动画
        MouseManager.Instance.Wait();

        int timespan = minute;
        CurTime = CurTime.AddMinutes(minute);

        while (timespan != 0)
        {
            if (timespan >= CurInterval)
            {
                timespan -= CurInterval;
                CurInterval = SETTLEMENT_INTERVAL;
                // ChatConditionManager.Instance.TrackCurrentStatus();
                EventManager.Instance.TriggerEvent(EventType.Update);
            }
            else
            {
                CurInterval -= timespan;
                timespan = 0;
            }
        }

        AnotherDay();

        EventManager.Instance.TriggerEvent(EventType.EndChangeTime);
    }

    public bool AnotherDay()
    {
        if (CurTime.Date == lastDay) return false;

        lastDay = CurTime.Date;
        EventManager.Instance.TriggerEvent(EventType.AnotherDay);
        return true;
    }
}