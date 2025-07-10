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
                    DontDestroyOnLoad(managerObj); // �糡������ʵ��
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        // ȷ��ֻ��һ��ʵ��
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

    //����ʱ�����
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
                EventManager.Instance.TriggerEvent(EventType.IntervalSettle);//����ʱ��״̬����
                //EventManager.Instance.TriggerEvent(EventType.RefreshCard);//�����;ö�ˢ��
            }
            else
            {
                curInterval -= time;
                time = 0;
            }
        }
    }

}