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

    [Header("已生成的对话列表")]
    public List<ChatData> GeneratedChatDataList = new List<ChatData>();//已生成的对话列表，存档&&读档用

    public List<int> ParagraphToTriggeer = new List<int>();//需要触发的对话列表
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
        if (Application.isPlaying)
        {
            DontDestroyOnLoad(gameObject);
        }
        EventManager.Instance.AddListener<int>(EventType.TriggerParagraph, TriggerParagraph);//触发某段对话
        GameDataManager.Instance.LoadGeneratedChatData();
        if (GeneratedChatDataList.Count == 0)//没有生成过对话时
        {
            ParagraphToTriggeer.Add(0);//添加新手引导
        }
        CSVReader.ReadChatData("test");
    }
    public void OnDestroy()
    {
        instance = null;
        GameDataManager.Instance.SaveGeneratedChatData();
        EventManager.Instance.RemoveListener<int>(EventType.TriggerParagraph, TriggerParagraph);
    }
    public void TriggerParagraph(int paragraphIndex)
    {
        ParagraphToTriggeer.Add(paragraphIndex);
    }
}