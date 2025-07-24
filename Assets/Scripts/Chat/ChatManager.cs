using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.IO;

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
                    if (Application.isPlaying)
                    {
                        DontDestroyOnLoad(managerObj);
                    }
                }
            }
            return instance;
        }
    }
    #endregion

    #region 数据
    [Header("对话数据")]
    public List<ParagraphData> ParagraphDataList = new List<ParagraphData>();

    [Header("预制体")]
    public GameObject NPCTextBox; // 作者消息文本框预制体
    public GameObject PlayerTextBox; // 玩家消息文本框预制体
    public GameObject AsideTextBox; // 旁白消息文本框预制体
    public GameObject MessagePrefab;//选项框预制体

    [Header("已生成的对话列表")]
    public List<ChatData> GeneratedChatDataList = new List<ChatData>();//已生成的对话列表，存档&&读档用

    public List<ParagraphData> ParagraphToTriggeer = new List<ParagraphData>();//需要触发的对话列表

    public ParagraphData CurrentParagraphData;//当前段落数据

    public bool canConfirm = false;//是否可以确认
    public ChatData ChoosedChatData;//当前对话数据
    public ChatData NextMessage=null;//下一个对话数据
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
        DontDestroyOnLoad(gameObject);
        EventManager.Instance.AddListener<ParagraphData>(EventType.TriggerParagraph, TriggerParagraph);
        ExcelReader.ReadChat("test");
        GameDataManager.Instance.LoadGeneratedChatData();
        if (GeneratedChatDataList.Count == 0)//没有生成过对话时
        {
            ParagraphToTriggeer.Add(ParagraphDataList[0]);//添加新手引导
        }

    }
    public void OnDestroy()
    {
        //GameDataManager.Instance.SaveGeneratedChatData();
        EventManager.Instance.RemoveListener<ParagraphData>(EventType.TriggerParagraph, TriggerParagraph);
    }
    public void TriggerParagraph(ParagraphData paragraphData)
    {
        ParagraphToTriggeer.Add(paragraphData);
    }
    public void TriggerChat(ChatData chatData)
    {
        NextMessage=chatData;
    }
}