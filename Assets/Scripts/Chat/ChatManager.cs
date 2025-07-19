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

    [Header("消息滚动区域")]
    public CustomVerticalLayout ChatArea; // 消息滚动区域
    public ScrollRect scroll; // 消息滚动条

    [Header("已生成的对话列表")]
    public List<ChatData> GeneratedChatDataList;//已生成的对话列表，存档&&读档用
    //已生成对话列表考虑用其他方法存储？

    // 私有成员变量
    private CustomVerticalLayout layout; // 自定义垂直布局组件
    private string SaveFolder; // 存档文件夹路径
    private string SaveFilePath; // 存档文件路径

    #endregion

    private void Awake()
    {
        // 初始化存档路径，需要根据存档读取，在项目中改到和其他json统一
        SaveFolder = Path.Combine(Application.persistentDataPath, "ChatData");//持久化路径文件夹
        SaveFilePath = Path.Combine(SaveFolder, "messages.json");//存档文件路径

        // 确保只有一个实例
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        //触发某段对话事件
        //触发所有已生成消息事件
        //触发点击某选项事件

    }
    public void OnDestroy()
    {
        instance = null;
        //触发某段对话事件
        //触发所有已生成消息事件
        //触发点击某选项事件
    }

    private void Start()
    {
        // 获取自定义布局组件
        layout = ChatArea.GetComponent<CustomVerticalLayout>();
        // 加载本地已生成消息key
        LoadGeneratedChatData();
    }


    public void LoadGeneratedChatData()
    {
        //从GeneratedChatDataList中加载已触发的对话数据
        foreach (var chatData in GeneratedChatDataList)
        {
            CreateMessage(chatData);
        }
    }

    //创建消息（不包括选项）
    public void CreateMessage(ChatData chatData)
    {
        //根据消息发送者选择对应的预制体
        GameObject MessagePrefab = null;
        switch(chatData.MessageSender)
        {
            case MessageSenderEnum.NPC:
               MessagePrefab=NPCTextBox;
                break;
            case MessageSenderEnum.Player:
                MessagePrefab=PlayerTextBox;
                break;
            case MessageSenderEnum.Aside:
                MessagePrefab=AsideTextBox;
                break;
        }
        
        if (MessagePrefab == null) return;
        
        //根据消息进行实例化
        GameObject MessageObject=Instantiate(MessagePrefab,ChatArea.transform);
        MessageObject.transform.localPosition = Vector3.zero;
        MessageObject.GetComponent<Text>().text=chatData.Message;

        //强制刷新文本框尺寸，确保显示正确
        MessageObject.GetComponent<CustomTextBox>()?.ForceRefreshSize();

        // 刷新布局，保证UI整齐
        layout.RefreshChildren();
        layout.RefreshAllTextBoxWidths();
        layout.UpdateLayout();

        // 滚动条自动滚动到底部，显示最新消息
        if (scroll != null) scroll.verticalNormalizedPosition = 0;
    }

    /// <summary>
    /// 创建选项消息（如"是/否"），并设置按钮事件
    /// chatData为单个选项消息
    /// </summary>
    public void CreateChooseMessage(ChatData chatData)
    {
        //根据消息进行实例化
        GameObject MessageObject=Instantiate(PlayerTextBox,ChatArea.transform);
        MessageObject.transform.localPosition = Vector3.zero;
        MessageObject.GetComponent<Text>().text=chatData.Message;

        //强制刷新文本框尺寸，确保显示正确
        MessageObject.GetComponent<CustomTextBox>()?.ForceRefreshSize();

        // 刷新布局，保证UI整齐
        layout.RefreshChildren();
        layout.RefreshAllTextBoxWidths();
        layout.UpdateLayout();

        //设置按钮事件
        Button button = MessageObject.GetComponent<Button>();
        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                //添加选项按钮监听
                StartCoroutine(DelayedChoose(chatData));
            });
        }
        
        // 滚动条自动滚动到底部，显示最新消息
        if (scroll != null) scroll.verticalNormalizedPosition = 0;
    }

    /// <summary>
    /// 延迟处理选项选择，销毁所有选项消息后生成新消息
    /// </summary>
    private IEnumerator DelayedChoose(ChatData chatData)
    {
        foreach (var button in GetComponentsInChildren<Button>())
        {
            Destroy(button.gameObject);
        }
        yield return null;
        CreateMessage(chatData);//生成被选择的消息    
        layout.RefreshChildren();
        layout.RefreshAllTextBoxWidths();
        layout.UpdateLayout();
    }

    //TODO：逐字显示
}