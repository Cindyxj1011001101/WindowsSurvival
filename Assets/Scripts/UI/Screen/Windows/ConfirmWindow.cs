using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConfirmWindow : WindowBase
{
    [SerializeField] private Text text;
    [SerializeField] private HoverableButton confirmButton;
    [SerializeField] private HoverableButton cancelButton;

    public UnityAction onConfirm;


    protected override void Init()
    {
        confirmButton.onClick.AddListener(() =>
        {
            WindowsManager.Instance.CloseWindow(AppName);
            onConfirm?.Invoke();
            onConfirm = null;
        });
        cancelButton.onClick.AddListener(() =>
        {
            WindowsManager.Instance.CloseWindow(AppName);
        });
    }

    public void SetText(string text)
    {
        this.text.text = text;
    }
}