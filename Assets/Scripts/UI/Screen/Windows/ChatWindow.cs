using DG.Tweening;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatWindow : WindowBase
{
    //[Header("组件")]
    //private GameObject layout;//聊天内容
    //private ScrollRect scroll;//滚动
    //private GameObject messageSpace;//选择区域
    //private GameObject ConfirmButton;//确认按钮
    //private GameObject InputText;//输入框
    //private GameObject body;
    //[Header("预制体")]
    //public GameObject NPCTextBox; // 作者消息文本框预制体
    //public GameObject PlayerTextBox; // 玩家消息文本框预制体
    //public GameObject AsideTextBox; // 旁白消息文本框预制体
    //public GameObject MessagePrefab;//选项框预制体


    //protected override void Init()
    //{        
    //    body = transform.Find("Body").gameObject;
    //    layout = transform.Find("Body/ScrollView/Viewport/Content").gameObject;
    //    scroll = transform.Find("Body/ScrollView").GetComponent<ScrollRect>();
    //    messageSpace = transform.Find("Body/MessageSpace").gameObject;
    //    ConfirmButton = transform.Find("Body/InputLine/Confirm").gameObject;
    //    InputText = transform.Find("Body/InputLine/InputBG/InputText").gameObject;

    //    NPCTextBox=Resources.Load<GameObject>("Prefabs/UI/Controls/Dialogue/NPC");
    //    PlayerTextBox=Resources.Load<GameObject>("Prefabs/UI/Controls/Dialogue/Player");
    //    AsideTextBox=Resources.Load<GameObject>("Prefabs/UI/Controls/Dialogue/Aside");
    //    MessagePrefab=Resources.Load<GameObject>("Prefabs/UI/Controls/Dialogue/Choose");

    //    ChatManager.Instance.chatWindow = this;
    //    //生成已发送过的对话数据
    //    ChatManager.Instance.InitChat();

    //    //刷新界面显示信息
    //    body.GetComponent<CustomMessageLayout>().Refresh();
    //    //添加确认按钮事件
    //    ConfirmButton.GetComponent<Button>().onClick.AddListener(Confirm);

    //}

    //public void OnDestroy()
    //{
    //    //存档当前对话数据
    //    GameDataManager.Instance.SaveGeneratedChatData();
    //}

    //public void CreateMessage(ChatData chatData)
    //{
    //    body.GetComponent<CustomMessageLayout>().Refresh();
    //    //根据消息发送者选择对应的预制体
    //    GameObject MessagePrefab = chatData.MessageSender switch
    //    {
    //        MessageSenderEnum.NPC => NPCTextBox,
    //        MessageSenderEnum.Player => PlayerTextBox,
    //        MessageSenderEnum.Aside => AsideTextBox,
    //        _ => null
    //    };
    //    //根据消息进行实例化
    //    GameObject MessageObject = Instantiate(MessagePrefab, layout.transform);
    //    MessageObject.GetComponentInChildren<Text>().text = chatData.Message;
    //    layout.GetComponent<CustomVerticalLayout>().RefreshAllChildren();
    //    if (scroll != null) scroll.verticalNormalizedPosition = 0;
    //}
    //public void CreateChooseMessagesSequentially(List<ChatData> options)
    //{
    //    foreach (var option in options)
    //    {
    //        //根据消息进行实例化
    //        GameObject MessageObject = Instantiate(MessagePrefab, messageSpace.transform);
    //        MessageObject.GetComponentInChildren<Text>().text = option.Message;

    //        //设置按钮事件
    //        Button button = MessageObject.AddComponent<Button>();
    //        if (button)
    //        {
    //            button.onClick.RemoveAllListeners();
    //            button.onClick.AddListener(() =>
    //            {
    //                //添加选项按钮监听
    //                Choose(button, option);
    //            });
    //        }
    //        //添加对话区域的监听
    //        body.transform.Find("InputLine/InputBG").GetComponent<Button>().onClick.AddListener(() =>
    //        {
    //            body.transform.Find("MessageSpace").gameObject.SetActive(true);
    //            body.GetComponent<CustomMessageLayout>().Refresh();
    //        });
    //    }
    //    body.GetComponent<CustomMessageLayout>().Refresh();
    //}
    ////选择按钮行为
    //private void Choose(Button aimbutton, ChatData chatData)
    //{
    //    if (ChatManager.Instance.ChoosedChatData == chatData)
    //    {
    //        Confirm();
    //        return;
    //    }
    //    foreach (var button in messageSpace.GetComponentsInChildren<Button>())
    //    {
    //        button.GetComponent<Image>().color = Color.blue;
    //    }
    //    aimbutton.GetComponent<Image>().color = Color.red;
    //    InputText.GetComponent<Text>().text = chatData.Message;
    //    ChatManager.Instance.ChoosedChatData = chatData;
    //    ChatManager.Instance.canConfirm = true;
    //}

    ////确认发送选项
    //public void Confirm()
    //{
    //    if (ChatManager.Instance.canConfirm)
    //    {
    //        //销毁所有选项消息
    //        foreach (var button in messageSpace.GetComponentsInChildren<Button>())
    //        {
    //            DestroyImmediate(button.gameObject);
    //        }
    //        InputText.GetComponent<Text>().text = "";
    //        //刷新界面显示
    //        body.GetComponent<CustomMessageLayout>().Refresh();
    //        //生成选中的消息
    //        StartCoroutine(ChatManager.Instance.CreateMessage(ChatManager.Instance.ChoosedChatData));
    //        //刷新数据存储
    //        ChatManager.Instance.canConfirm = false;
    //        ChatManager.Instance.ChoosedChatData = null;
    //        //移除对话区域的监听
    //        body.transform.Find("InputLine/InputBG").GetComponent<Button>().onClick.RemoveAllListeners();
    //    }
    //}

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
        //点击输入区域
        inputFieldButton.onClick.AddListener(ShowDialogueOptions);

        // 点击发送消息
        submitButton.onClick.AddListener(Submit);

        //ChatManager.Instance.chatWindow = this;
        ////生成已发送过的对话数据
        //ChatManager.Instance.InitChat();
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
            var rectTransform = Instantiate(optionPrefab, optionLayout).transform as RectTransform;
            var text = rectTransform.GetComponent<Text>();
            text.text = option.Message;
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, (text.transform as RectTransform).sizeDelta.y);
            var button = rectTransform.GetComponent<HoverableButton>();
            button.onClick.AddListener(() =>
            {
                inputFieldText.text = option.Message;
                ChatManager.Instance.ChoosedChatData = option;
            });
        }

        ShowDialogueOptions();
    }

    /// <summary>
    /// 显示对话选项
    /// </summary>
    public void ShowDialogueOptions()
    {
        if (seq != null && seq.IsActive()) return;

        if (optionLayout.gameObject.activeSelf) return;

        seq = DOTween.Sequence();

        seq.Join(typeArea.DOSizeDelta(new Vector2(typeArea.sizeDelta.x, (inputFieldButton.transform as RectTransform).sizeDelta.y + optionLayout.sizeDelta.y - 2), .3f))
           .Join(chatScrollViewRect.DOSizeDelta(new Vector2(chatScrollViewRect.sizeDelta.x, chatScrollViewRect.sizeDelta.y - optionLayout.sizeDelta.y + 2), .3f))
           .OnComplete(() => optionLayout.gameObject.SetActive(true));
    }

    /// <summary>
    /// 隐藏对话选项
    /// </summary>
    public void HideDialogueOptions()
    {
        if (seq != null && seq.IsActive()) return;

        if (!optionLayout.gameObject.activeSelf) return;

        seq = DOTween.Sequence();

        seq.OnStart(() => optionLayout.gameObject.SetActive(false))
           .Join(typeArea.DOSizeDelta(new Vector2(typeArea.sizeDelta.x, (inputFieldButton.transform as RectTransform).sizeDelta.y), .3f))
           .Join(chatScrollViewRect.DOSizeDelta(new Vector2(chatScrollViewRect.sizeDelta.x, chatScrollViewRect.sizeDelta.y + optionLayout.sizeDelta.y - 2), .3f));
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
        HideDialogueOptions();
    }
}