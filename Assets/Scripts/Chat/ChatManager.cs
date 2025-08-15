using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Experimental.GraphView;
using UnityEngine.SceneManagement;
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

    //已生成的对话列表
    public List<ChatData> GeneratedChatDataList = new List<ChatData>();

    //需要触发的段落列表(存储段落名)
    public List<string> ParagraphToTriggeer = new List<string>();

    //当前段落数据
    public ParagraphData CurrentParagraphData=>ReadChatParagraph.Instance.CurGraphData.paragraphData;

    //当前选项数据
    public string ChoosedChatData;

    //是否在段落中
    public bool inParagraph = false;

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
        //读取已生成的对话数据
        GameDataManager.Instance.LoadGeneratedChatData();
        //添加对话段落触发监听
        EventManager.Instance.AddListener<ParagraphData>(EventType.TriggerParagraph, TriggerParagraph);
        if (!GameDataManager.Instance.GeneratedChatData.init)
        {
            if (!GameDataManager.Instance.LoadData.loads[GameDataManager.Instance.curLoadIndex].SkipGuide)
            {
                ParagraphToTriggeer.Add("一切的开始");
            }
        }
        else
        {
            GeneratedChatDataList = GameDataManager.Instance.GeneratedChatData.GeneratedChatDataList;
            ParagraphToTriggeer = GameDataManager.Instance.GeneratedChatData.ParagraphToTriggeer;
            inParagraph = GameDataManager.Instance.GeneratedChatData.inParagraph;
            InterruptParagraphData = GameDataManager.Instance.GeneratedChatData.InterruptParagraphData;
            Choosing = GameDataManager.Instance.GeneratedChatData.Choosing;


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
                    TriggerMessage(null);
                }
            }
            else
            {
                ParagraphToTriggeer.Add(paragraphData.ParagraphName);
            }
        }
        else
        {
            TriggerParagraph(paragraphData);
        }
    }

    public void TriggerParagraph(ParagraphData paragraphData)
    {
        inParagraph = true;
        ReadChatParagraph.Instance.FindStartNodeOfParagraph(paragraphData.ParagraphName);
        TriggerMessage(ReadChatParagraph.Instance.CurNode);
    }

    //生成所有被记录的数据
    public void LoadGeneratedChatData()
    {
        //进入对话
        inParagraph = true;
        //从GeneratedChatDataList中加载所有已触发的对话数据
        for (int i = 0; i < GeneratedChatDataList.Count; i++)
        {
            chatWindow.CreateMessage(GeneratedChatDataList[i].MessageSender, GeneratedChatDataList[i].Message);
        }

        //触发下一个对话（找到最后一句的下一句）
        if (ReadChatParagraph.Instance.CurNode.typeName == "End")
        {
            NextParagraph();
        }
        else
        {
            TriggerMessage(ReadChatParagraph.Instance.FindNextNode());
        }

    }

    //根据下一条消息的类型决定触发消息类型为选项还是消息
    public void TriggerMessage(GraphData.SerializedNode nodeData)
    {
        //如果打断对话非空时触发打断对话
        if (InterruptParagraphData != null)
        {
            ParagraphData tmpParagraph = InterruptParagraphData;
            InterruptParagraphData = null;
            TriggerParagraph(tmpParagraph);
            return;
        }

        //如果需要判断通过条件
        if (nodeData.chatData.MessageCondition != "")
        {
            inParagraph = false;
            ChatConditionManager.Instance.StartChatConditionDetection(nodeData);
            return;
        }

        //根据类型生成消息
        switch (nodeData.typeName)
        {
            case "Dialogue":
                CreateMessage(nodeData.chatData);
                break;
            case "Choose":
                Choosing = true;
                chatWindow.SetDialogueOptions(nodeData);
                break;
            case "BranchCondition":
                // // 先收集所有选项消息
                // List<ChatData> branchOptionsList = new List<ChatData>();
                // for (int i = chatData.MessageID - 1; i < ParagraphDataList[chatData.ParagraphID - 1].ChatDataList.Count; i++)
                // {
                //     if (ParagraphDataList[chatData.ParagraphID].ChatDataList[i].MessageType == "分支对话")
                //     {
                //         branchOptionsList.Add(ParagraphDataList[chatData.ParagraphID].ChatDataList[i]);
                //     }
                //     else break;
                // }
                // foreach (var option in branchOptionsList)
                // {
                //     if (option.MessageCondition != "" && ChatConditionManager.Instance.CanTriggerBranchCondition(option))
                //     {
                //         CreateMessage(option);
                //         break;
                //     }
                // }
                break;
            case "End":
                NextParagraph();
                break;
        }
    }

    public void AddToGenerated(ChatData chatData)
    {
        GeneratedChatDataList.Add(chatData);
        if (GeneratedChatDataList.Count > 20)
        {
            GeneratedChatDataList.RemoveAt(0);
            chatWindow.RemoveFirstMessage();
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
        else finalWaitTime = chatData.preWaitTime == 0 ? 2.5f : chatData.preWaitTime;

        finalWaitTime /= curSpeed;

        yield return new WaitForSeconds(finalWaitTime);

        //将该对话加入已生成列表
        AddToGenerated(chatData);
        chatWindow.CreateMessage(chatData.MessageSender, chatData.Message);

        SoundManager.Instance.PlaySound("消息提示音_02", true);
        //触发对话效果
        AfterChatFactory.TriggerEffect(chatData.TriggerMessageEffect);

        //消息前置等待时间
        if (waitTime > 0) finalWaitTime = waitTime;
        else finalWaitTime = chatData.preWaitTime == 0 ? 2.5f : chatData.preWaitTime;
        finalWaitTime /= curSpeed;
        yield return new WaitForSeconds(finalWaitTime);

        TriggerMessage(ReadChatParagraph.Instance.FindNextNode());

    }

    public void NextParagraph()
    {
        if (ParagraphToTriggeer.Count > 0)
        {
            TriggerParagraph(ReadChatParagraph.Instance.FindParagraphDataByName(ParagraphToTriggeer[0]));
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
        TriggerMessage(ReadChatParagraph.Instance.FindNextNode(ChoosedChatData));
        ChoosedChatData = null;
    }

    private void ChangeChatSpeed(int speed)
    {
        curSpeed = speed;
        chatSpeedButton.GetComponentInChildren<Text>().text = $"x{speed}";
    }

    public void ReturnToMainMenuAndDeleteSave()
    {
        int index = GameDataManager.Instance.curLoadIndex;
        //删除本存档
        GameDataManager.Instance.LoadData.loads[index] = null;
        GameDataManager.Instance.SaveLoadData();
        //目标路径
        string targetFolder = Application.persistentDataPath + "/GameData" + index + "/";
        // 如果目标文件夹不存在，先创建
        if (Directory.Exists(targetFolder))
        {
            Directory.Delete(targetFolder, true);
        }
        else
        {
            Debug.Log("存档不存在");
            return;
        }

        //返回初始界面
        SceneManager.LoadScene(0);
    }
}