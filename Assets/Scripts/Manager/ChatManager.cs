using UnityEngine;
using System.Collections.Generic;

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

    public List<ChatData> ChatDataList;

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

    public void LoadChatData()
    {
    }


}