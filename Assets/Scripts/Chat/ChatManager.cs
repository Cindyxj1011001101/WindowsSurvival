using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    #region 单例
    private static ChatManager instance;
    public static ChatManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ChatManager>();
                if (instance == null)
                {
                    GameObject managerObj = new GameObject("ChatManager");
                    instance = managerObj.AddComponent<ChatManager>();
                }
            }
            return instance;
        }
    }
    #endregion

    public ChatWindow chatWindow;

    public HoverableButton chatSpeedButton;
    private int curSpeed = 1;

    #region 数据
    //所有对话数据
    public List<ParagraphData> ParagraphDataList = new List<ParagraphData>();
    //已生成的对话列表
    public List<ChatData> GeneratedChatDataList = new List<ChatData>();
    //需要触发的段落列表
    public List<ParagraphData> ParagraphToTriggeer = new List<ParagraphData>();
    //当前段落数据
    public ParagraphData CurrentParagraphData;
    //当前选项数据
    public ChatData ChoosedChatData;
    //是否在段落中
    private bool inParagraph = false;
    //打断的段落数据
    public ParagraphData InterruptParagraphData = null;
    //当前是否在选择中
    public bool Choosing = false;
    #endregion

    private void Awake()
    {
        // 确保只有一个实例
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        //读取对话数据
        ExcelReader.ReadChat("ChatData");
        //读取已生成的对话数据
        GameDataManager.Instance.LoadGeneratedChatData();
        //添加对话段落触发监听
        EventManager.Instance.AddListener<ParagraphData>(EventType.TriggerParagraph, TriggerParagraph);
        //没有生成过对话时
        if (GeneratedChatDataList.Count == 0)
        {
            //触发新手引导对话
            ParagraphToTriggeer.Add(ParagraphDataList[0]);
        }

    }

    private void Start()
    {
        ChangeChatSpeed(1);
        chatSpeedButton.onClick.AddListener(() =>
        {
            if (curSpeed == 1)
            {
                ChangeChatSpeed(3);
            }
            else if (curSpeed == 3)
            {
                ChangeChatSpeed(10);
            }
            else
            {
                ChangeChatSpeed(1);
            }
        });
    }
    public void OnDestroy()
    {
        //移除对话段落监听
        EventManager.Instance.RemoveListener<ParagraphData>(EventType.TriggerParagraph, TriggerParagraph);
    }

    public void InitChat()
    {
        if (GeneratedChatDataList.Count > 0)
        {
            LoadGeneratedChatData();
        }
        else if (ParagraphToTriggeer.Count > 0)
        {
            NextParagraph();
        }
    }
    //触发段落时判断是否需要打断，不打断则放弃该段对话
    public void AddTriggerParagraph(ParagraphData paragraphData)
    {
        //当前在段落内
        if (inParagraph)
        {
            //判断是否可以打断，无法打断则加入待触发列表，在本段对话结束后触发
            if (paragraphData.ParagraphPriority > CurrentParagraphData.ParagraphPriority)
            {
                InterruptParagraphData = paragraphData;
                //如果当前在等待选择则删除选项，直接进入对话
                if (Choosing)
                {
                    chatWindow.InterruptChoose();
                    Choosing = false;
                }
            }
            else
            {
                ParagraphToTriggeer.Add(paragraphData);
            }
        }
        else
        {
            TriggerParagraph(paragraphData);
        }
    }
    public void TriggerParagraph(ParagraphData paragraphData)
    {
        InterruptParagraphData = null;
        CurrentParagraphData = paragraphData;
        inParagraph = true;
        TriggerMessage(paragraphData.ChatDataList[0]);
    }
    //生成所有被记录的数据
    public void LoadGeneratedChatData()
    {
        //进入对话
        inParagraph = true;
        CurrentParagraphData = ParagraphDataList[GeneratedChatDataList[GeneratedChatDataList.Count - 1].ParagraphID];
        //从GeneratedChatDataList中加载已触发的对话数据
        for (int i = 0; i < GeneratedChatDataList.Count - 1; i++)
        {
            chatWindow.CreateMessage(GeneratedChatDataList[i].MessageSender, GeneratedChatDataList[i].Message);
        }
        TriggerMessage(GeneratedChatDataList[GeneratedChatDataList.Count - 1]);
    }
    //根据下一条消息的类型决定触发消息类型为选项还是消息
    public void TriggerMessage(ChatData chatData)
    {
        //如果打断对话非空时触发打断对话
        if (InterruptParagraphData != null)
        {
            TriggerParagraph(InterruptParagraphData);
            InterruptParagraphData = null;
            return;
        }
        if (chatData == null) return;
        //非分支对话且有条件时开始该条件判断，不生成对话
        if (chatData.MessageCondition != "" && chatData.MessageType != "分支对话")
        {
            inParagraph = false;
            ChatConditionManager.Instance.StartChatConditionDetection(chatData);
            return;
        }
        //根据类型生成消息
        switch (chatData.MessageType)
        {
            case "对话":
                CreateMessage(chatData);
                break;
            case "选项":
                // 先收集所有选项消息
                List<ChatData> optionsList = new List<ChatData>();
                for (int i = chatData.MessageID - 1; i < ParagraphDataList[chatData.ParagraphID].ChatDataList.Count; i++)
                {
                    if (ParagraphDataList[chatData.ParagraphID].ChatDataList[i].MessageType == "选项")
                    {
                        optionsList.Add(ParagraphDataList[chatData.ParagraphID].ChatDataList[i]);
                    }
                    else break;
                }
                Choosing = true;
                chatWindow.SetDialogueOptions(optionsList);
                break;
            case "分支对话":
                // 先收集所有选项消息
                List<ChatData> branchOptionsList = new List<ChatData>();
                for (int i = chatData.MessageID - 1; i < ParagraphDataList[chatData.ParagraphID - 1].ChatDataList.Count; i++)
                {
                    if (ParagraphDataList[chatData.ParagraphID].ChatDataList[i].MessageType == "分支对话")
                    {
                        branchOptionsList.Add(ParagraphDataList[chatData.ParagraphID].ChatDataList[i]);
                    }
                    else break;
                }
                foreach (var option in branchOptionsList)
                {
                    if (option.MessageCondition != "" && ChatConditionManager.Instance.CanTriggerBranchCondition(option))
                    {
                        CreateMessage(option);
                        break;
                    }
                }
                break;
            case "提示":
                CreateMessage(chatData);
                break;
        }
    }

    public void CreateMessage(ChatData chatData, float waitTime = -1f)
    {
        StartCoroutine(CreateMessageCoroutine(chatData, waitTime));
    }

    //创建消息（不包括选项）
    private IEnumerator CreateMessageCoroutine(ChatData chatData, float waitTime)
    {
        float finalWaitTime;
        if (waitTime > 0) finalWaitTime = waitTime;
        else finalWaitTime = chatData.WaitTime == 0 ? 2.5f : chatData.WaitTime / 1000;

        finalWaitTime /= curSpeed;
        
        yield return new WaitForSeconds(finalWaitTime);

        //将该对话加入已生成列表
        GeneratedChatDataList.Add(chatData);
        chatWindow.CreateMessage(chatData.MessageSender, chatData.Message);

        SoundManager.Instance.PlaySound("消息提示音_02", true);
        AfterChatFactory.TriggerEffect(chatData.TriggerMessageEffect);
        //yield return new WaitForSeconds(0.5f);

        //触发对话效果
        if (chatData.NextMessageID != -1)
        {
            TriggerMessage(ParagraphDataList[chatData.ParagraphID].ChatDataList[chatData.NextMessageID - 1]);
        }
        else
        {
            NextParagraph();
        }
    }

    public void NextParagraph()
    {
        if (ParagraphToTriggeer.Count > 0)
        {
            TriggerParagraph(ParagraphToTriggeer[0]);
            ParagraphToTriggeer.RemoveAt(0);
        }
        else
        {
            inParagraph = false;
        }
    }

    public void Submit()
    {
        if (ChoosedChatData == null) return;
        Choosing = false;
        CreateMessage(ChoosedChatData, .1f); // 0.1f是玩家主动发送消息的发送延迟
        ChoosedChatData = null;
    }

    private void ChangeChatSpeed(int speed)
    {
        curSpeed = speed;
        chatSpeedButton.GetComponentInChildren<Text>().text = $"x{speed}";
    }
}