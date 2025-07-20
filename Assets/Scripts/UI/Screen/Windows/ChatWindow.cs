using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections;

public class ChatWindow : WindowBase
{
    private GameObject layout;
    private ScrollRect scroll;
    private bool inParagraph=false;
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
        if(!inParagraph&&ChatManager.Instance.ParagraphToTriggeer.Count>0)
        {
            TriggerParagraph(ChatManager.Instance.ParagraphToTriggeer[0]);
            ChatManager.Instance.ParagraphToTriggeer.RemoveAt(0);
        }
    }
    public void LoadGeneratedChatData()
    {
        //从GeneratedChatDataList中加载已触发的对话数据(一次性)
        for(int i=0;i<ChatManager.Instance.GeneratedChatDataList.Count-1;i++)
        {
            CreateNewMessage(ChatManager.Instance.GeneratedChatDataList[i]);
        }
        TriggerMessage(ChatManager.Instance.GeneratedChatDataList[ChatManager.Instance.GeneratedChatDataList.Count-1]);
    }

    public void TriggerParagraph(int paragraphIndex)
    {
        foreach (var paragraphData in ChatManager.Instance.ParagraphDataList)
        {
            if (paragraphData.ParagraphID == paragraphIndex)
            {
                inParagraph=true;
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
                for (int i = chatData.MessageID - 1; i < ChatManager.Instance.ParagraphDataList[chatData.ParagraphID - 1].ChatDataList.Count; i++)
                {
                    if (ChatManager.Instance.ParagraphDataList[chatData.ParagraphID - 1].ChatDataList[i].MessageType == 2)
                    {
                        StartCoroutine(CreateChooseMessage(ChatManager.Instance.ParagraphDataList[chatData.ParagraphID - 1].ChatDataList[i]));
                    }
                    else break;
                }
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
        GameObject MessageObject=CreateNewMessage(chatData);

        //等待时间后，如果存在下一条消息，则创建下一条消息
        yield return new WaitForSeconds(1f);
        if (chatData.NextMessageID != -1)
        {
            TriggerMessage(ChatManager.Instance.ParagraphDataList[chatData.ParagraphID - 1].ChatDataList[chatData.NextMessageID - 1]);
        }
        else
        {
            inParagraph=false;
        }
    }

    //
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
        MessageObject.transform.localPosition = Vector3.zero;
        MessageObject.GetComponentInChildren<TMP_Text>().text = chatData.Message;

        //刷新布局
        layout.GetComponent<CustomVerticalLayout>().RefreshChildren();
        layout.GetComponent<CustomVerticalLayout>().RefreshAllTextBoxWidths();
        layout.GetComponent<CustomVerticalLayout>().UpdateLayout();

        // 滚动条自动滚动到底部，显示最新消息
        if (scroll != null) scroll.verticalNormalizedPosition = 0;
        return MessageObject;
    }
    /// <summary>
    /// 创建选项消息，并设置按钮事件
    /// chatData为单个选项消息
    /// </summary>
    public IEnumerator CreateChooseMessage(ChatData chatData)
    {
        GameObject MessageObject=CreateNewMessage(chatData);

        //设置按钮事件
        Button button = MessageObject.transform.GetChild(0).gameObject.AddComponent<Button>();
        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                //添加选项按钮监听
                DelayedChoose(chatData);
            });
        }

        yield return null;
    }

    /// <summary>
    /// 延迟处理选项选择，销毁所有选项消息后生成新消息
    /// </summary>
    private void DelayedChoose(ChatData chatData)
    {
        foreach (var button in layout.GetComponentsInChildren<Button>())
        {
            button.transform.parent.gameObject.SetActive(false);
            Destroy(button.transform.parent.gameObject);
        }
        layout.GetComponent<CustomVerticalLayout>().RefreshChildren();
        layout.GetComponent<CustomVerticalLayout>().RefreshAllTextBoxWidths();
        layout.GetComponent<CustomVerticalLayout>().UpdateLayout();
        //StartCoroutine(WaitSeconds(1f));
        StartCoroutine(CreateMessage(chatData));//生成被选择的消息    
    }

    private IEnumerator WaitSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    //TODO：逐字显示



}