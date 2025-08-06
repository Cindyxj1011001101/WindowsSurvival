using System;
using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    public DateTime StartDateTime { get; private set; } = new(2020, 1, 1, 0, 0, 0);
    public DateTime curTime;
    public int SettleInterval;
    public int curInterval;
    private DateTime lastDay;

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
            curTime = StartDateTime;
            curInterval = SettleInterval;
        }
        else
        {
            //从存档初始化
            curTime = GameDataManager.Instance.TimeData.curTime;
            curInterval = GameDataManager.Instance.TimeData.curIntervel;
        }

        // 初始化lastDay
        lastDay = curTime.Date;
    }

    private void Start()
    {
        EventManager.Instance.TriggerEvent(EventType.ChangeTime, curTime);
    }

    public void AddTime(int minute)
    {
        // 等待动画
        MouseManager.Instance.Wait();

        int time = minute;
        curTime = curTime.AddMinutes(minute);
        EventManager.Instance.TriggerEvent(EventType.ChangeTime, curTime);
        while (time != 0)
        {
            if (time >= curInterval)
            {
                time -= curInterval;
                curInterval = SettleInterval;
                EventManager.Instance.TriggerEvent(EventType.IntervalSettle);
                EventManager.Instance.TriggerEvent(EventType.ChangeCardProperty);
            }
            else
            {
                curInterval -= time;
                time = 0;
            }
        }
        lastDay = curTime.Date;
    }

    public bool AnotherDay()
    {
        return curTime.Date != lastDay;
    }
}