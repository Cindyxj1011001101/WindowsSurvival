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
    { get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ChatManager>();
                if (instance == null)
                {
                    GameObject managerObj = new GameObject("ChatManager");
                    instance = managerObj.AddComponent<ChatManager>();
                    DontDestroyOnLoad(managerObj); // 跨场景保持实例
                }
            }
            return instance;
        }
    }
    #endregion

    #region 数据
    [Header("对话数据")]
    public List<ParagraphData> ParagraphDataList;

    [Header("预制体")]
    public GameObject NPCTextBox; // 作者消息文本框预制体
    public GameObject PlayerTextBox; // 玩家消息文本框预制体
    public GameObject AsideTextBox; // 旁白消息文本框预制体

    [Header("已生成的对话列表")]
    public List<ChatData> GeneratedChatDataList;//已生成的对话列表，存档&&读档用

    [Header("存档路径")]
    public string SaveFolder; // 存档文件夹路径
    public string SaveFilePath; // 存档文件路径
    //已生成对话列表考虑用其他方法存储？

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

    }
    public void OnDestroy()
    {
        instance = null;
    }

   

}