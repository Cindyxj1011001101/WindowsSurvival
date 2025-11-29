using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CustomWindow : WindowBase
{
    [SerializeField] private Text content;
    [SerializeField] private RectTransform buttonLayout;
    [SerializeField] private GameObject buttonPrefab;

    protected override void Init()
    {
    }

    private void OnDisable()
    {
        ObjectBufferPool.Instance.RestoreAllChildren(buttonLayout);
    }

    public void SetContent(string content)
    {
        this.content.text = content;
    }

    public void AddButton(string text, UnityAction onClick, bool closeWindowAfterClick = true)
    {
        var button = ObjectBufferPool.Instance.Get(buttonPrefab, buttonLayout).GetComponent<HoverableButton>();
        button.text.text = text;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            onClick?.Invoke();
            if (closeWindowAfterClick)
                WindowsManager.Instance.CloseWindow(AppName);
        });
        button.AdaptWidth();
        MonoUtility.UpdateLayoutSize(buttonPrefab.GetComponent<ILayoutGroup>());
    }

    public void ConfirmAndCancel(UnityAction onConfirm, UnityAction onCancel = null)
    {
        AddButton("确认", onConfirm, true);
        AddButton("取消", onCancel, true);
    }
}