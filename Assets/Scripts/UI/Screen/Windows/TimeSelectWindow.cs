using UnityEngine;
using DanielLochner.Assets.SimpleScrollSnap;
using UnityEngine.UI;

public class TimeSelectWindow : WindowBase
{
    [SerializeField] private SimpleScrollSnap hourScroll;
    [SerializeField] private SimpleScrollSnap minuteScroll;

    protected override void Init()
    {
        //hourScroll.OnPanelSelected.AddListener((index) =>
        //{
        //    Debug.Log($"Hour: Selected panel index: {index}");
        //});
        //minuteScroll.OnPanelSelected.AddListener((index) =>
        //{
        //    Debug.Log($"Minute: Selected panel index: {index}");
        //});
        hourScroll.OnPanelCentered.AddListener((index, _) =>
        {
            Debug.Log($"Hour: {23 - index}");
        });
        minuteScroll.OnPanelCentered.AddListener((index, _) =>
        {
            Debug.Log($"Minute: {59 - index}");
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
    }
}