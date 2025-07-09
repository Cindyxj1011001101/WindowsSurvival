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
                Init();
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
                                Debug.Log("Interval");
                                EventManager.Instance.TriggerEvent(EventType.IntervalSettle);//处理时间状态结算
                                EventManager.Instance.TriggerEvent(EventType.RefreshCard);//卡牌耐久度刷新
                        }
                        else
                        {
                                curInterval -= time;
                                time = 0;
                        }
                }
        }
        
}