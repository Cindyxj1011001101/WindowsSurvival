using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class ChatWindow : WindowBase
{
    private GameObject layout;
    private ScrollRect scroll;
    private GameObject messageSpace;
    private GameObject ConfirmButton;
    private GameObject InputText;
    private bool inParagraph = false;
    private GameObject body;
    public ParagraphData InterruptParagraphData = null;//打断的段落数据
    protected override void Start()
    {
        base.Start();

    }

    protected override void Init()
    {
        layout = transform.Find("Body/Scroll View/Viewport/Content").gameObject;
        scroll = transform.Find("Body/Scroll View").GetComponent<ScrollRect>();
        messageSpace = transform.Find("Body/MessageSpace").gameObject;
        ConfirmButton = transform.Find("Body/InputLine/Confirm").gameObject;
        InputText = transform.Find("Body/InputLine/InputBG/InputText").gameObject;
        body = transform.Find("Body").gameObject;
        body.GetComponent<CustomMessageLayout>().Refresh();
        ConfirmButton.GetComponent<Button>().onClick.RemoveAllListeners();
        ConfirmButton.GetComponent<Button>().onClick.AddListener(Confirm);
    }

    public void OnDestroy()
    {
        //存档当前对话数据
        GameDataManager.Instance.SaveGeneratedChatData();
    }

    public void Update()
    {
        //检测是否有待触发段落
        if (ChatManager.Instance.ParagraphToTriggeer.Count > 0)
        {
            //遍历可触发段落
            foreach (var paragraph in ChatManager.Instance.ParagraphToTriggeer)
            {
                //优先级高时打断当前段落，并触发新段落
                if (ChatManager.Instance.CurrentParagraphData != null && paragraph.ParagraphPriority > ChatManager.Instance.CurrentParagraphData.ParagraphPriority)
                {
                    InterruptParagraphData = paragraph;
                    ChatManager.Instance.ParagraphToTriggeer.Remove(paragraph);
                    break;
                }
                //当前不在段落中且段落为空时直接触发
                else if (!inParagraph && ChatManager.Instance.CurrentParagraphData == null)
                {
                    TriggerParagraph(paragraph);
                    ChatManager.Instance.ParagraphToTriggeer.Remove(paragraph);
                    break;
                }
            }
        }
        if (ChatManager.Instance.NextMessage != null)
        {
            StartCoroutine(CreateMessage(ChatManager.Instance.NextMessage));
            ChatManager.Instance.NextMessage = null;
        }
    }
    public void LoadGeneratedChatData()
    {
        //从GeneratedChatDataList中加载已触发的对话数据(一次性)
        for (int i = 0; i < ChatManager.Instance.GeneratedChatDataList.Count - 1; i++)
        {
            CreateNewMessage(ChatManager.Instance.GeneratedChatDataList[i]);
        }
        TriggerMessage(ChatManager.Instance.GeneratedChatDataList[ChatManager.Instance.GeneratedChatDataList.Count - 1]);
    }

    public void TriggerParagraph(ParagraphData paragraphData)
    {
        ChatManager.Instance.CurrentParagraphData = paragraphData;
        inParagraph = true;
        TriggerMessage(paragraphData.ChatDataList[0]);

    }

    //根据下一条消息的类型决定触发消息类型为选项还是消息
    public void TriggerMessage(ChatData chatData)
    {
        if (InterruptParagraphData != null)
        {
            TriggerParagraph(InterruptParagraphData);
            InterruptParagraphData = null;
        }
        if (chatData.MessageCondition != "" && chatData.MessageType != "分支对话")
        {
            ChatConditionManager.Instance.StartChatConditionDetection(chatData);
            return;
        }
        switch (chatData.MessageType)
        {
            case "对话":
                StartCoroutine(CreateMessage(chatData));
                break;
            case "选项":
                // 先收集所有选项消息
                List<ChatData> optionsList = new List<ChatData>();
                for (int i = chatData.MessageID - 1; i < ChatManager.Instance.ParagraphDataList[chatData.ParagraphID - 1].ChatDataList.Count; i++)
                {
                    if (ChatManager.Instance.ParagraphDataList[chatData.ParagraphID - 1].ChatDataList[i].MessageType == "选项")
                    {
                        optionsList.Add(ChatManager.Instance.ParagraphDataList[chatData.ParagraphID - 1].ChatDataList[i]);
                    }
                    else break;
                }
                CreateChooseMessagesSequentially(optionsList);
                break;
            case "提示":
                StartCoroutine(CreateMessage(chatData));
                break;
        }
    }

    public IEnumerator WaitBeforeMessage(float waitTime)
    {
        //可能需要加动画
        yield return new WaitForSeconds(waitTime);
    }

    //创建消息（不包括选项）
    public IEnumerator CreateMessage(ChatData chatData)
    {
        StartCoroutine(WaitBeforeMessage(chatData.WaitTime));
        ChatManager.Instance.GeneratedChatDataList.Add(chatData);
        GameObject MessageObject = CreateNewMessage(chatData);

        yield return new WaitForSeconds(0.5f);
        if (chatData.NextMessageID != -1)
        {
            TriggerMessage(ChatManager.Instance.ParagraphDataList[chatData.ParagraphID - 1].ChatDataList[chatData.NextMessageID - 1]);
        }
        else
        {
            inParagraph = false;
        }
    }

    //

    public GameObject CreateNewMessage(ChatData chatData)
    {
        body.GetComponent<CustomMessageLayout>().Refresh();
        //根据消息发送者选择对应的预制体
        GameObject MessagePrefab = null;
        switch (chatData.MessageSender)
        {
            case MessageSenderEnum.NPC:
                MessagePrefab = ChatManager.Instance.NPCTextBox;
                break;
            case MessageSenderEnum.Player:
                MessagePrefab = ChatManager.Instance.PlayerTextBox;
                break;
            case MessageSenderEnum.Aside:
                MessagePrefab = ChatManager.Instance.AsideTextBox;
                break;
        }
        //根据消息进行实例化
        GameObject MessageObject = Instantiate(MessagePrefab, layout.transform);
        MessageObject.GetComponentInChildren<Text>().text = chatData.Message;
        layout.GetComponent<CustomVerticalLayout>().RefreshAllChildren();
        if (scroll != null) scroll.verticalNormalizedPosition = 0;
        return MessageObject;
    }

    public void CreateChooseMessagesSequentially(List<ChatData> options)
    {
        foreach (var option in options)
        {
            //根据消息进行实例化
            GameObject MessageObject = Instantiate(ChatManager.Instance.MessagePrefab, messageSpace.transform);
            MessageObject.GetComponentInChildren<Text>().text = option.Message;

            //设置按钮事件
            Button button = MessageObject.AddComponent<Button>();
            if (button)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    //添加选项按钮监听
                    Choose(button, option);
                });
            }
        }
        body.GetComponent<CustomMessageLayout>().Refresh();
    }
    private void Choose(Button aimbutton, ChatData chatData)
    {
        //被点击的按钮变化状态，其余所有按钮恢复默认
        //输入框显示被点击按钮的文字
        //发送按钮可被点击
        if (ChatManager.Instance.ChoosedChatData == chatData) return;
        foreach (var button in messageSpace.GetComponentsInChildren<Button>())
        {
            button.GetComponent<Image>().color = Color.blue;
        }
        aimbutton.GetComponent<Image>().color = Color.red;
        InputText.GetComponent<Text>().text = chatData.Message;
        ChatManager.Instance.ChoosedChatData = chatData;
        ChatManager.Instance.canConfirm = true;
    }

    public void Confirm()
    {
        if (ChatManager.Instance.canConfirm)
        {
            //销毁所有选项消息
            foreach (var button in messageSpace.GetComponentsInChildren<Button>())
            {
                Destroy(button.gameObject);
            }
            InputText.GetComponent<Text>().text = "";
            //刷新界面显示
            body.GetComponent<CustomMessageLayout>().Refresh();
            //生成选中的消息
            StartCoroutine(CreateMessage(ChatManager.Instance.ChoosedChatData));
            //刷新数据存储
            ChatManager.Instance.canConfirm = false;
            ChatManager.Instance.ChoosedChatData = null;
        }
    }
    private IEnumerator WaitSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    //TODO：逐字显示



}