using UnityEngine;
using UnityEngine.UI;

public class EventTipWindow : WindowBase
{
    [SerializeField] private Text contentText;
    [SerializeField] private Text titleText;
    [SerializeField] private Image iconImage;
    [SerializeField] private HoverableButton closeButton;

    protected override void Init()
    {
        closeButton.onClick.AddListener(() =>
        {
            WindowsManager.Instance.CloseWindow(AppName);
        });
    }

    public void SetTitle(Sprite icon, string title, Color color)
    {
        iconImage.sprite = icon;
        titleText.text = title;
        iconImage.color = titleText.color = color;
    }

    public void SetContent(string content)
    {
        contentText.text = content;
    }
}