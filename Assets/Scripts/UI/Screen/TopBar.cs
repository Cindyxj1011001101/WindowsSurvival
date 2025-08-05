using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TopBar : MonoBehaviour
{
    [SerializeField] private Text dateText;
    [SerializeField] private Text timeText;
    [SerializeField] private HoverableButton saveButton;
    [SerializeField] private HoverableButton restButton;

    private void Awake()
    {
        EventManager.Instance.AddListener<DateTime>(EventType.ChangeTime, OnTimeChanged);
    }

    private void Start()
    {
        saveButton.onClick.AddListener(() =>
        {
            GameDataManager.Instance.SaveAllData();
            SceneManager.LoadScene(0);
        });

        restButton.onClick.AddListener(() =>
        {
            var window = WindowsManager.Instance.OpenWindow("TimeSelect", true) as TimeSelectWindow;
            window.onConfirm += (time) =>
            {
                StateManager.Instance.Sleep(time);
                Debug.Log($"休息了{time}分钟");
            };
        });
    }

    private void OnTimeChanged(DateTime dateTime)
    {
        if (dateText != null)
        {
            dateText.text = CalculateDate(dateTime);
        }
        if (timeText != null)
        {
            timeText.text = CalculateTime(dateTime);
        }
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