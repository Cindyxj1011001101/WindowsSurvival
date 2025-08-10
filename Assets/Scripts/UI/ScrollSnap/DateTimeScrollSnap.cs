using DanielLochner.Assets.SimpleScrollSnap;
using System;
using UnityEngine;
using UnityEngine.UI;

public class DateTimeScrollSnap : MonoBehaviour
{
    [SerializeField] private SimpleScrollSnap dateScrollSnap;
    [SerializeField] private SimpleScrollSnap hourScrollSnap;
    [SerializeField] private SimpleScrollSnap minuteScrollSnap;

    private DateTime lastDateTime;

    private void Awake()
    {
        EventManager.Instance.AddListener<DateTime>(EventType.ChangeTime, UpdateTime);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<DateTime>(EventType.ChangeTime, UpdateTime);
    }

    private void Start()
    {
        Init();
        dateScrollSnap.OnPanelCentered.AddListener((_, _) =>
        {
            for (int i = 0; i < dateScrollSnap.Content.childCount; i++)
            {
                SetChildText(dateScrollSnap.CenteredPanel + i, (TimeManager.Instance.Day + i).ToString());
            }
        });
    }

    public void Init()
    {
        lastDateTime = TimeManager.Instance.CurTime;


        for (int i = 0; i < dateScrollSnap.Content.childCount; i++)
        {
            SetChildText(dateScrollSnap.CenteredPanel + i, (TimeManager.Instance.Day + i).ToString());
        }

        foreach (Transform child in hourScrollSnap.Content)
        {
            if (child.TryGetComponent<Text>(out var text))
            {
                text.text = $"{ child.GetSiblingIndex():D2}";
            }
        }
        foreach (Transform child in minuteScrollSnap.Content)
        {
            if (child.TryGetComponent<Text>(out var text))
            {
                text.text = $"{child.GetSiblingIndex():D2}";
            }
        }

        hourScrollSnap.StartingPanel = TimeManager.Instance.Hour;
        minuteScrollSnap.StartingPanel = TimeManager.Instance.Minute;
    }

    private void SetChildText(int index, string text)
    {
        index = GetChildIndex(dateScrollSnap, index);
        dateScrollSnap.Content.GetChild(index).GetComponent<Text>().text = text;
    }

    private void UpdateTime(DateTime curTime)
    {
        var m = curTime.Minute - lastDateTime.Minute;
        var h = curTime.Hour - lastDateTime.Hour;
        var d = curTime.Day - lastDateTime.Day;

        lastDateTime = curTime;
        
        minuteScrollSnap.GoToPanel(GetChildIndex(minuteScrollSnap, m + minuteScrollSnap.CenteredPanel));
        hourScrollSnap.GoToPanel(GetChildIndex(hourScrollSnap, h + hourScrollSnap.CenteredPanel));
        dateScrollSnap.GoToPanel(GetChildIndex(dateScrollSnap, d + dateScrollSnap.CenteredPanel));
    }

    private int GetChildIndex(SimpleScrollSnap scroll, int index)
    {
        return (index % scroll.Content.childCount + scroll.Content.childCount) % scroll.Content.childCount;
    }
}