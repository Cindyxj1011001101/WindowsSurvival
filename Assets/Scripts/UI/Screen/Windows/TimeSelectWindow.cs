using UnityEngine;
using DanielLochner.Assets.SimpleScrollSnap;
using UnityEngine.UI;
using UnityEngine.Events;

public class TimeSelectWindow : WindowBase
{
    [SerializeField] private SimpleScrollSnap hourScroll;
    [SerializeField] private SimpleScrollSnap minuteScroll;

    [SerializeField] private HoverableButton confirmButton;
    [SerializeField] private HoverableButton cancelButton;

    private int hour = 0;
    private int minute = 0;

    public UnityAction<int> onConfirm;

    protected override void Init()
    {
        hourScroll.OnPanelCentered.AddListener((current, previous) =>
        {
            hour = 23 - current;
        });
        minuteScroll.OnPanelCentered.AddListener((current, previous) =>
        {
            minute = 59 - current;
        });
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
        hour = 23 - hourScroll.StartingPanel;
        minute = 59 - minuteScroll.StartingPanel;

        confirmButton.onClick.AddListener(() =>
        {
            onConfirm?.Invoke(hour * 60 + minute);
            WindowsManager.Instance.CloseWindow(AppName);
        });
        cancelButton.onClick.AddListener(() =>
        {
            WindowsManager.Instance.CloseWindow(AppName);
        });
    }
}