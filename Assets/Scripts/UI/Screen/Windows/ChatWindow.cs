using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatWindow : WindowBase
{
    [SerializeField] private GameObject playerChatPrefab;
    [SerializeField] private GameObject othersChatPrefab;
    [SerializeField] private GameObject narrtionPrefab;

    [SerializeField] private ChatLayoutGroup chatLayoutGroup;
    [SerializeField] private RectTransform chatScrollViewRect;

    [SerializeField] private RectTransform typeArea;

    [SerializeField] private Button inputFieldButton;
    [SerializeField] private Text inputFieldText;

    [SerializeField] private HoverableButton submitButton;

    [SerializeField] private RectTransform optionLayout;
    [SerializeField] private GameObject optionPrefab;

    private Sequence seq;

    protected override void Init()
    {
        inputFieldText.text = "";

        //点击输入区域
        inputFieldButton.onClick.AddListener(ShowDialogueOptions);

        // 点击发送消息
        submitButton.onClick.AddListener(Submit);

        ChatManager.Instance.chatWindow = this;
        //生成已发送过的对话数据
        ChatManager.Instance.InitChat();

        ResetScroll();
    }

    /// <summary>
    /// 添加一条对话
    /// </summary>
    /// <param name="sender"></param>
    public void CreateMessage(MessageSenderEnum sender, string content)
    {
        GameObject prefab = null;
        switch (sender)
        {
            case MessageSenderEnum.NPC:
                prefab = othersChatPrefab;
                break;
            case MessageSenderEnum.Player:
                prefab = playerChatPrefab;
                break;
            case MessageSenderEnum.Aside:
                prefab = narrtionPrefab;
                break;
        }

        // 创建聊天气泡
        Instantiate(prefab, chatLayoutGroup.transform).GetComponent<CustomTextBox>().SetText(content);

        // 更新组件高度
        MonoUtility.UpdateChatLayoutSize(chatLayoutGroup);

        ResetScroll();
    }

    private void ResetScroll()
    {
        // 设置滚动到底部
        Canvas.ForceUpdateCanvases();
        chatScrollViewRect.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
    }

    /// <summary>
    /// 设置对话选项
    /// </summary>
    /// <param name="options"></param>
    public void SetDialogueOptions(List<ChatData> options)
    {
        MonoUtility.DestroyAllChildren(optionLayout);
        foreach (var option in options)
        {
            var button = Instantiate(optionPrefab, optionLayout).GetComponent<DialogueOption>();
            button.SetText(option.Message);
            button.onClick.AddListener(() =>
            {
                inputFieldText.text = option.Message;
                ChatManager.Instance.ChoosedChatData = option;
            });
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(optionLayout);

        ShowDialogueOptions();
    }

    float animDuration = 0.15f;
    /// <summary>
    /// 显示对话选项
    /// </summary>
    public void ShowDialogueOptions()
    {
        if (optionLayout.gameObject.activeSelf) return;

        if (seq != null && seq.IsActive()) seq.Kill();

        chatScrollViewRect.sizeDelta = new Vector2(chatScrollViewRect.sizeDelta.x, chatScrollViewRect.sizeDelta.y - optionLayout.sizeDelta.y + 2);

        ResetScroll();

        seq = DOTween.Sequence();

        seq.Join(typeArea.DOSizeDelta(new Vector2(typeArea.sizeDelta.x, (inputFieldButton.transform as RectTransform).sizeDelta.y + optionLayout.sizeDelta.y - 2), animDuration))
           .OnComplete(() => optionLayout.gameObject.SetActive(true));
    }

    /// <summary>
    /// 隐藏对话选项
    /// </summary>
    public void HideDialogueOptions()
    {
        if (!optionLayout.gameObject.activeSelf) return;

        if (seq != null && seq.IsActive()) seq.Kill();

        seq = DOTween.Sequence();

        seq.OnStart(() => optionLayout.gameObject.SetActive(false))
           .Join(chatScrollViewRect.DOSizeDelta(new Vector2(chatScrollViewRect.sizeDelta.x, chatScrollViewRect.sizeDelta.y + optionLayout.sizeDelta.y - 2), animDuration))
           .Join(typeArea.DOSizeDelta(new Vector2(typeArea.sizeDelta.x, (inputFieldButton.transform as RectTransform).sizeDelta.y), animDuration));
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        // 点击到聊天区域，隐藏选项
        var currentObject = eventData.pointerCurrentRaycast.gameObject;
        if (currentObject.name == chatScrollViewRect.gameObject.name)
        {
            // 隐藏选项
            HideDialogueOptions();
        }
    }

    private void Submit()
    {
        if (string.IsNullOrEmpty(inputFieldText.text)) return;

        ChatManager.Instance.Submit();
        inputFieldText.text = "";
        MonoUtility.DestroyAllChildren(optionLayout);
    }
}