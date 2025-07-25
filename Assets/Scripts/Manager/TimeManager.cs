using System;
using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    public DateTime StartDateTime { get; private set; } = new (2020, 1, 1, 0, 0, 0);
    public DateTime curTime;
    public int SettleInterval;
    public int curInterval;

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
        //默认初始化
        curTime = StartDateTime;
        curInterval = SettleInterval;

        //从存档初始化
        curTime=GameDataManager.Instance.GameRuntimeData.CurTime;
        curInterval=GameDataManager.Instance.GameRuntimeData.CurInterval;
    }

    private void Start()
    {
        EventManager.Instance.TriggerEvent(EventType.ChangeTime, curTime);
    }


    public void AddTime(int minute)
    {
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
    }
}