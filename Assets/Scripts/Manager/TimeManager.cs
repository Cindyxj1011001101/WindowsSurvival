using System;
using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{

    public DateTime curTime;
    public int SettleInterval;
    public int curInterval;
    private TMP_Text dateText;
    private TMP_Text timeText;

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
                    DontDestroyOnLoad(managerObj);
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
        DontDestroyOnLoad(gameObject);
        Init();
    }
    public void Init()
    {
        curTime = new DateTime(2020, 1, 1, 0, 0, 0);
        curInterval = SettleInterval;
        dateText = GameObject.Find("Time/Date").GetComponent<TMP_Text>();
        timeText = GameObject.Find("Time/Time").GetComponent<TMP_Text>();
        dateText.text = CalculateDate();
        timeText.text = CalculateTime();
    }
    public string CalculateDate()
    {
        DateTime startDate = new DateTime(2020, 1, 1);
        TimeSpan timeSpan = curTime - startDate;
        int days = timeSpan.Days+1;
        return $"{days}Days";
    }

    public string CalculateTime()
    {
        int hour = curTime.Hour;
        int minute = curTime.Minute;
        return $"{hour:D2}:{minute:D2}";
    }

    
    public void AddTime(int minute)
    {
        int time = minute;
        curTime = curTime.AddMinutes(minute);
        dateText.text = CalculateDate();
        timeText.text = CalculateTime();
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