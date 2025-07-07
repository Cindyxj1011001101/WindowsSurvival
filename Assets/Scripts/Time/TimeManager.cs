using System;
using UnityEngine;

public class TimeManager:MonoBehaviour
{
        public DateTime curTime;
        public int SettleInterval;
        public int curInterval;

        private static TimeManager instance;
        public static TimeManager Instance => instance;

        private void Awake()
        {
                instance = this;
                DontDestroyOnLoad(gameObject);
        }
        public void Init()
        {
                curTime = new DateTime(2020, 1, 1, 0, 0,0);
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
                                curInterval=SettleInterval;
                                EventManager.Instance.TriggerEvent(EventType.IntervalSettle);
                        }
                        else
                        {
                                curInterval -= time;
                                time = 0;
                        }
                }
        }
        
}