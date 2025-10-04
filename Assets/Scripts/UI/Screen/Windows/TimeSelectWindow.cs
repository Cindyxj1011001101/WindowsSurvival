using UnityEngine;
using DanielLochner.Assets.SimpleScrollSnap;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

public class TimeSelectWindow : WindowBase
{
    [SerializeField] private SimpleScrollSnap hourScroll;
    [SerializeField] private SimpleScrollSnap minuteScroll;

    [SerializeField] private HoverableButton confirmButton;
    [SerializeField] private HoverableButton cancelButton;

    [SerializeField] private HoverableButton hourUpButton;
    [SerializeField] private HoverableButton hourDownButton;
    [SerializeField] private HoverableButton minuteUpButton;
    [SerializeField] private HoverableButton minuteDownButton;

    private int hour = 0;
    private int minute = 0;

    public UnityAction<int> onConfirm;
    public OutStringAction<bool> canConfirm;

    public Func<int, (string, int, Dictionary<PlayerStateEnum, float>, Dictionary<EnvironmentStateEnum, float>)> getConfirmEffects;

    private int minMinute = 0;
    private int minHour = 0;

    private int maxMinute = 60;
    private int maxHour = 24;

    public void SetTimeRange(int minTime, int maxTime)
    {
        minHour = minTime / 60;
        minMinute = minTime % 60;
        maxHour = maxTime / 60;
        maxMinute = maxTime % 60;

        for (int i = 0; i < hourScroll.Content.childCount; i++)
        {
            hourScroll.Content.GetChild(23 - i).gameObject.SetActive(i >= minHour && i <= maxHour);
        }

        hour = minHour;
        minute = minMinute;
        hourScroll.StartingPanel = 23 - minHour;
        minuteScroll.StartingPanel = 59 - minMinute;
        ClampTimeRange();
    }

    protected override void Awake()
    {
        base.Awake();

        foreach (Transform child in hourScroll.Content)
        {
            if (child.TryGetComponent<Text>(out var text))
            {
                text.text = $"{23 - child.GetSiblingIndex():D2}";
            }
        }
        foreach (Transform child in minuteScroll.Content)
        {
            if (child.TryGetComponent<Text>(out var text))
            {
                text.text = $"{59 - child.GetSiblingIndex():D2}";
            }
        }
    }

    protected override void Init()
    {
        minuteUpButton.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySound("简单点击_01", true);
            if (!minuteScroll.Content.GetChild((minuteScroll.CenteredPanel + 1 + 60) % 60).gameObject.activeSelf) return;

            minuteScroll.GoToNextPanel();
            minute = (minute - 1 + 60) % 60;
        });

        minuteDownButton.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySound("简单点击_01", true);
            if (!minuteScroll.Content.GetChild((minuteScroll.CenteredPanel - 1 + 60) % 60).gameObject.activeSelf) return;

            minuteScroll.GoToPreviousPanel();
            minute = (minute + 1 + 60) % 60;
        });

        hourUpButton.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySound("简单点击_01", true);
            if (!hourScroll.Content.GetChild((hourScroll.CenteredPanel + 1 + 24) % 24).gameObject.activeSelf) return;

            hourScroll.GoToNextPanel();
            hour = (hour - 1 + 24) % 24;
            ClampTimeRange();
        });

        hourDownButton.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySound("简单点击_01", true);
            if (!hourScroll.Content.GetChild((hourScroll.CenteredPanel - 1 + 24) % 24).gameObject.activeSelf) return;

            hourScroll.GoToPreviousPanel();
            hour = (hour + 1 + 24) % 24;
            ClampTimeRange();
        });

        cancelButton.onClick.AddListener(() =>
        {
            WindowsManager.Instance.CloseWindow(AppName);
        });
        confirmButton.onClick.AddListener(() =>
        {
            WindowsManager.Instance.CloseWindow(AppName);
            onConfirm?.Invoke(hour * 60 + minute);
            onConfirm = null;
        });
        // 确定按钮是否可用交互
        string hint = string.Empty;
        confirmButton.Interactable = canConfirm == null || canConfirm.Invoke(out hint);

        var tipController = confirmButton.GetComponent<HoverTipController>();

        // 显示不可交互的原因
        if (!confirmButton.Interactable)
        {
            tipController.SetTip(hint);
            return;
        }

        if (getConfirmEffects == null) return;

        tipController.onPointerEnter.AddListener(() =>
        {
            (string textTip, int time, Dictionary<PlayerStateEnum, float> p, Dictionary<EnvironmentStateEnum, float> e) = getConfirmEffects.Invoke(hour * 60 + minute);
            tipController.SetTip(textTip, time, p, e);
        });
    }

    private void ClampTimeRange()
    {
        if (hour == maxHour)
        {
            for (int i = 0; i < minuteScroll.Content.childCount; i++)
            {
                minuteScroll.Content.GetChild(59 - i).gameObject.SetActive(i <= maxMinute);
            }
            if (minute > maxMinute)
            {
                minute = maxMinute;
                minuteScroll.GoToPanel(59 - maxMinute);
            }
        }
        else if (hour == minHour)
        {
            for (int i = 0; i < minuteScroll.Content.childCount; i++)
            {
                minuteScroll.Content.GetChild(59 - i).gameObject.SetActive(i >= minMinute);
            }
            if (minute < minMinute)
            {
                minute = minMinute;
                minuteScroll.GoToPanel(59 - minMinute);
            }
        }
        else
        {
            for (int i = 0; i < minuteScroll.Content.childCount; i++)
            {
                minuteScroll.Content.GetChild(i).gameObject.SetActive(true);
            }
        }
    }
}