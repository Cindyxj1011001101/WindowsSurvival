using UnityEngine;

public class ChatManager : MonoBehaviour
{
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

    //进入游戏-加载保存当前进度的对话-是否段落结束/等待选择判断，未结束继续运行弹出对话-段落结束
    //1.加载保存当前进度的对话
    //2.判断是否需要继续加载对话（对话中，开始运行时）
    //3.运行对话到某句


}