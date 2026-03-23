using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using System.Linq;
using Random = UnityEngine.Random;

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

    // 已进行过的剧情段落（只记录段落名，不记录具体对话文本）
    public List<string> StoryProgressParagraphs = new List<string>();

    // LLM每轮返回的概括（会持久化到存档）
    public string LLMPreviousSummary = "无";

    // 自动唤起LLM（游戏内分钟）
    public int AutoLLMElapsedMinutes = 0;
    public int AutoLLMTargetMinutes = -1;
    public bool EnableAutoLLMWakeup = true;

    private const int AUTO_LLM_MIN_MINUTES = 3;
    private const int AUTO_LLM_MAX_MINUTES = 10;

    // 是否正在自动输出剧情文本（消息协程进行中）
    public bool IsStoryOutputting { get; private set; } = false;

    /// <summary>
    /// 是否处于剧情状态
    /// 剧情状态下只允许节点推进，不允许自由聊天
    /// </summary>
    public bool IsInStoryState =>
        Choosing ||
        IsStoryOutputting;

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

        //添加对话段落触发监听
        EventManager.Instance.AddListener<ParagraphData>(EventType.TriggerParagraph, TriggerParagraph);
        if (!GameDataManager.Instance.GeneratedChatData.init)
        {
            if (!GameDataManager.Instance.LoadData.loads[GameDataManager.Instance.curLoadIndex].skipGuide)
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
            StoryProgressParagraphs = GameDataManager.Instance.GeneratedChatData.StoryProgressParagraphs ?? new List<string>();
            LLMPreviousSummary = string.IsNullOrWhiteSpace(GameDataManager.Instance.GeneratedChatData.LLMPreviousSummary)
                ? "无"
                : GameDataManager.Instance.GeneratedChatData.LLMPreviousSummary;
            AutoLLMElapsedMinutes = GameDataManager.Instance.GeneratedChatData.AutoLLMElapsedMinutes;
            AutoLLMTargetMinutes = GameDataManager.Instance.GeneratedChatData.AutoLLMTargetMinutes;
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

        EnsureAutoLLMTarget();
        EventManager.Instance.AddListener(EventType.FineUpdate, OnFineUpdate);
    }

    public void OnDestroy()
    {
        //移除对话段落监听
        EventManager.Instance.RemoveListener<ParagraphData>(EventType.TriggerParagraph, TriggerParagraph);
        EventManager.Instance.RemoveListener(EventType.FineUpdate, OnFineUpdate);
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
                    ChoosedChatData = null;
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
        if (paragraphData != null && !string.IsNullOrEmpty(paragraphData.ParagraphName))
        {
            if (!StoryProgressParagraphs.Contains(paragraphData.ParagraphName))
                StoryProgressParagraphs.Add(paragraphData.ParagraphName);
        }
        ReadChatParagraph.Instance.FindStartNodeOfParagraph(paragraphData.ParagraphName);
        TriggerMessage(ReadChatParagraph.Instance.CurNode);
    }

    public string GetStoryProgressForPrompt()
    {
        if (StoryProgressParagraphs == null || StoryProgressParagraphs.Count == 0)
            return "无";

        return string.Join(" | ", StoryProgressParagraphs.Where(p => !string.IsNullOrEmpty(p)));
    }

    private void EnsureAutoLLMTarget()
    {
        if (AutoLLMTargetMinutes < AUTO_LLM_MIN_MINUTES || AutoLLMTargetMinutes > AUTO_LLM_MAX_MINUTES)
            AutoLLMTargetMinutes = Random.Range(AUTO_LLM_MIN_MINUTES, AUTO_LLM_MAX_MINUTES + 1);
    }

    private void OnFineUpdate()
    {
        if (!EnableAutoLLMWakeup) return;

        EnsureAutoLLMTarget();
        AutoLLMElapsedMinutes++;

        if (AutoLLMElapsedMinutes < AutoLLMTargetMinutes) return;
        if (IsInStoryState) return;
        if (chatWindow == null) return;

        bool started = chatWindow.TryAutoInvokeLLM();
        if (!started) return;

        AutoLLMElapsedMinutes = 0;
        AutoLLMTargetMinutes = Random.Range(AUTO_LLM_MIN_MINUTES, AUTO_LLM_MAX_MINUTES + 1);
    }

    //生成所有被记录的数据
    public void LoadGeneratedChatData()
    {
        //进入对话
        inParagraph = true;
        //从GeneratedChatDataList中加载所有已触发的对话数据
        for (int i = 0; i < GeneratedChatDataList.Count-1; i++)
        {
            chatWindow.CreateMessage(GeneratedChatDataList[i].MessageSender, GeneratedChatDataList[i].Message);
        }
        //触发下一个对话（找到当前节点的下一句，如果最后一句是选项或分支需要重新触发最后一句的效果）
        if (ReadChatParagraph.Instance.CurNode.typeName == "End")
        {
            NextParagraph();
        }
        else if(ReadChatParagraph.Instance.CurNode.typeName=="Choose"||ReadChatParagraph.Instance.CurNode.typeName=="BranchCondition")
        {
            TriggerMessage(ReadChatParagraph.Instance.CurNode);
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
                if (nodeData.chatData.MessageCondition != "")
                {
                    inParagraph = false;
                    ChatConditionManager.Instance.StartChatConditionDetection(nodeData);
                    return;
                }
                CreateMessage(nodeData.chatData);
                break;
            case "Choose":
                Choosing = true;
                chatWindow.SetDialogueOptions(nodeData);
                break;
            case "BranchCondition":
                // 先收集所有选项消息
                foreach (var portData in nodeData.outputports)
                {
                    if (portData.name != "" && ChatConditionManager.Instance.CanTriggerBranchCondition(portData.name))
                    {
                        TriggerMessage(ReadChatParagraph.Instance.FindNextNode(portData.name));
                        break;
                    }
                }
                break;
            case "End":
                NextParagraph();
                break;
            case "Start":
                TriggerMessage(ReadChatParagraph.Instance.FindNextNode());
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
        IsStoryOutputting = true;

        float finalWaitTime;

        if (waitTime > 0) finalWaitTime = waitTime;
        else finalWaitTime = chatData.preWaitTime == 0 ? 0.2f : chatData.preWaitTime;

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
        else finalWaitTime = chatData.lateWaitTime == 0 ? 2.1f : chatData.lateWaitTime;
        finalWaitTime /= curSpeed;
        yield return new WaitForSeconds(finalWaitTime);

        IsStoryOutputting = false;
        TriggerMessage(ReadChatParagraph.Instance.FindNextNode());

    }

    public void NextParagraph()
    {
        if (ParagraphToTriggeer.Count > 0)
        {
            ParagraphData tmpParagraphData=ReadChatParagraph.Instance.FindParagraphDataByName(ParagraphToTriggeer[0]);
            TriggerParagraph(tmpParagraphData);
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

        // 防御：剧情图状态异常时，不执行跳转，避免空引用
        if (ReadChatParagraph.Instance == null || ReadChatParagraph.Instance.CurGraphData == null || ReadChatParagraph.Instance.CurNode == null)
        {
            Debug.LogWarning("[ChatManager] 当前不在有效剧情节点，忽略本次选项提交。");
            Choosing = false;
            ChoosedChatData = null;
            return;
        }

        Choosing = false;
        var nextNode = ReadChatParagraph.Instance.FindNextNode(ChoosedChatData);
        if (nextNode == null)
        {
            Debug.LogWarning($"[ChatManager] 选项 \"{ChoosedChatData}\" 未找到下一节点，忽略本次提交。");
            ChoosedChatData = null;
            return;
        }

        TriggerMessage(nextNode);
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
        MySceneManager.LoadScene(0);
    }
}
