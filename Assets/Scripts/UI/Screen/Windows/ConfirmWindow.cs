using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConfirmWindow : WindowBase
{
    [SerializeField] private Image icon;
    [SerializeField] private Text title;
    [SerializeField] private Text content;
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

    public void SetContent(string content)
    {
        this.content.text = content;
    }

    public void SetHeadline(Sprite icon, string title, Color color)
    {
        this.icon.sprite = icon;
        this.title.text = title;
        this.icon.color = this.title.color = color;
    }

    public void DisableCancelButton()
    {
        cancelButton.gameObject.SetActive(false);
    }
}