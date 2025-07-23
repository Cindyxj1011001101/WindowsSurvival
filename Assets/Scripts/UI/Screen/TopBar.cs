using System;
using UnityEngine;
using UnityEngine.UI;

public class TopBar : MonoBehaviour
{
    [SerializeField] private Text dateText;
    [SerializeField] private Text timeText;

    private void Awake()
    {
        EventManager.Instance.AddListener<DateTime>(EventType.ChangeTime, OnTimeChanged);
    }

    private void OnTimeChanged(DateTime dateTime)
    {
        dateText.text = CalculateDate(dateTime);
        timeText.text = CalculateTime(dateTime);
    }

    public string CalculateDate(DateTime curTime)
    {
        TimeSpan timeSpan = curTime - TimeManager.Instance.StartDateTime;
        int days = timeSpan.Days + 1;
        return $"Day {days}";
    }

    public string CalculateTime(DateTime curTime)
    {
        int hour = curTime.Hour;
        int minute = curTime.Minute;
        return $"{hour:D2}:{minute:D2}";
    }
}