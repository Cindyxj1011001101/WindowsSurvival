using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{

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
        Init();
    }
    public void Init()
    {
        curTime = new DateTime(2020, 1, 1, 0, 0, 0);
        curInterval = SettleInterval;
    }

    //增加时间结算
    public void AddTime(int minute)
    {
        int time = minute;
        curTime.AddMinutes(minute);
        while (time != 0)
        {
            if (time >= curInterval)
            {
                time -= curInterval;
                curInterval = SettleInterval;
                EventManager.Instance.TriggerEvent(EventType.IntervalSettle);//处理时间状态结算
                //EventManager.Instance.TriggerEvent(EventType.RefreshCard);//卡牌耐久度刷新
            }
            else
            {
                curInterval -= time;
                time = 0;
            }
        }
    }

}