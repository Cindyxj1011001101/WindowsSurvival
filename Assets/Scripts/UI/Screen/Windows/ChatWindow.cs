using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class ChatWindow : WindowBase
{
    private GameObject layout;
    private ScrollRect scroll;
    private bool inParagraph = false;
    protected override void Start()
    {
        base.Start();

    }

    protected override void Init()
    {
        layout = transform.Find("Body/Scroll View/Viewport/Content").gameObject;
        scroll = transform.Find("Body/Scroll View").GetComponent<ScrollRect>();

        TriggerParagraph(1);
    }

    public void OnDestroy()
    {
        //存档当前对话数据
        GameDataManager.Instance.SaveGeneratedChatData();
    }

    public void Update()
    {
        if (!inParagraph && ChatManager.Instance.ParagraphToTriggeer.Count > 0)
        {
            TriggerParagraph(ChatManager.Instance.ParagraphToTriggeer[0]);
            ChatManager.Instance.ParagraphToTriggeer.RemoveAt(0);
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

    public void TriggerParagraph(int paragraphIndex)
    {
        foreach (var paragraphData in ChatManager.Instance.ParagraphDataList)
        {
            if (paragraphData.ParagraphID == paragraphIndex)
            {
                inParagraph = true;
                TriggerMessage(paragraphData.ChatDataList[0]);
            }
        }

    }

    //根据下一条消息的类型决定触发消息类型为选项还是消息
    public void TriggerMessage(ChatData chatData)
    {
        switch (chatData.MessageType)
        {
            case 1://对话
                StartCoroutine(CreateMessage(chatData));
                break;
            case 2://选项
                   // 先收集所有选项消息
                List<ChatData> optionsList = new List<ChatData>();
                for (int i = chatData.MessageID - 1; i < ChatManager.Instance.ParagraphDataList[chatData.ParagraphID - 1].ChatDataList.Count; i++)
                {
                    if (ChatManager.Instance.ParagraphDataList[chatData.ParagraphID - 1].ChatDataList[i].MessageType == 2)
                    {
                        optionsList.Add(ChatManager.Instance.ParagraphDataList[chatData.ParagraphID - 1].ChatDataList[i]);
                    }
                    else break;
                }
                StartCoroutine(CreateChooseMessagesSequentially(optionsList));
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
        yield return new WaitForSeconds(1f);
        if (chatData.NextMessageID != -1)
        {
            TriggerMessage(ChatManager.Instance.ParagraphDataList[chatData.ParagraphID - 1].ChatDataList[chatData.NextMessageID - 1]);
        }
        else
        {
            inParagraph = false;
        }
    }

    public GameObject CreateNewMessage(ChatData chatData)
    {
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
        MessageObject.GetComponentInChildren<TMP_Text>().text = chatData.Message;

        StartCoroutine(ScrollToBottomNextFrame());
        return MessageObject;
    }

    private IEnumerator ScrollToBottomNextFrame()
    {
        for (int i = 0; i < 2; i++)
            yield return null;
        layout.GetComponent<CustomVerticalLayout>().RefreshAllChildren();
        if (scroll != null) scroll.verticalNormalizedPosition = 0;
    }

    public IEnumerator CreateChooseMessagesSequentially(List<ChatData> options)
    {
        foreach (var option in options)
        {
            GameObject MessageObject = CreateNewMessage(option);

            //设置按钮事件
            Button button = MessageObject.AddComponent<Button>();
            button.transition = Button.Transition.None;
            if (button)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    //添加选项按钮监听
                    DelayedChoose(option);
                });
            }
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// 延迟处理选项选择，销毁所有选项消息后生成新消息
    /// </summary>
    private void DelayedChoose(ChatData chatData)
    {
        foreach (var button in layout.GetComponentsInChildren<Button>())
        {
            button.gameObject.SetActive(false);
            Destroy(button.gameObject);
        }
        StartCoroutine(CreateMessage(chatData));//生成被选择的消息    
    }

    private IEnumerator WaitSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    //TODO：逐字显示



}