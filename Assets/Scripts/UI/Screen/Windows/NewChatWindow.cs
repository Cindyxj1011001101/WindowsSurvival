using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NewChatWindow : WindowBase
{
    [SerializeField] private GameObject playerChatPrefab;
    [SerializeField] private GameObject othersChatPrefab;
    [SerializeField] private GameObject narrtionPrefab;

    [SerializeField] private ChatLayoutGroup chatLayoutGroup;
    [SerializeField] private RectTransform chatScrollViewRect;

    [SerializeField] private Button inputFieldButton;
    [SerializeField] private Text inputFieldText;

    [SerializeField] private HoverableButton submitButton;

    [SerializeField] private RectTransform optionLayout;
    [SerializeField] private GameObject optionPrefab;

    private IEnumerator Test()
    {
        yield return new WaitForSecondsRealtime(3);
        AddChat(MessageSenderEnum.Player, "你好");
        yield return new WaitForSecondsRealtime(1);
        AddChat(MessageSenderEnum.Player, "你好哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈");
        yield return new WaitForSecondsRealtime(1);
        AddChat(MessageSenderEnum.NPC, "我很好");
        yield return new WaitForSecondsRealtime(1);
        AddChat(MessageSenderEnum.Aside, "365里路呀");
        yield return new WaitForSecondsRealtime(1);
        AddChat(MessageSenderEnum.Player, "很高兴认识你");
        yield return new WaitForSecondsRealtime(1);
    }

    protected override void Init()
    {
        // 读取已经进行的对话


        // 点击输入区域
        inputFieldButton.onClick.AddListener(() =>
        {
            ShowDialogueOptions();
        });

        // 点击发送消息
        submitButton.onClick.AddListener(() =>
        {
            if (string.IsNullOrEmpty(inputFieldText.text))
                return;
            else
                AddChat(MessageSenderEnum.Player, inputFieldText.text);
        });

        StartCoroutine(Test());
    }

    /// <summary>
    /// 添加一条对话
    /// </summary>
    /// <param name="sender"></param>
    public void AddChat(MessageSenderEnum sender, string content)
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

        Instantiate(prefab, chatLayoutGroup.transform).GetComponent<CustomTextBox>().SetText(content);

        MonoUtility.UpdateChatLayoutSize(chatLayoutGroup);
    }

    /// <summary>
    /// 设置对话选项
    /// </summary>
    /// <param name="options"></param>
    public void SetDialogueOptions(List<string> options)
    {
        MonoUtility.DestroyAllChildren(optionLayout);
        foreach (string option in options)
        {
            var rectTransform = Instantiate(optionPrefab, optionLayout).transform as RectTransform;
            var text = rectTransform.GetComponent<Text>();
            text.text = option;
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, (text.transform as RectTransform).sizeDelta.y);
            var button = rectTransform.GetComponent<HoverableButton>();
            button.onClick.AddListener(() =>
            {
                inputFieldText.text = option;
            });
        }
        ShowDialogueOptions();
    }

    /// <summary>
    /// 显示对话选项
    /// </summary>
    public void ShowDialogueOptions()
    {
        optionLayout.gameObject.SetActive(true);
    }

    /// <summary>
    /// 隐藏对话选项
    /// </summary>
    public void HideDialogueOptions()
    {
        optionLayout.gameObject.SetActive(false);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        // 点击到聊天区域，隐藏选项
        var currentObject = eventData.pointerCurrentRaycast.gameObject;
        if (currentObject.name == chatScrollViewRect.gameObject.name)
        {
            // 隐藏选项
            Debug.Log("隐藏");
        }
    }
}