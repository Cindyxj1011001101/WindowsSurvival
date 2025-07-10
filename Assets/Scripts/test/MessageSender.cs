using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Linq;
using UnityEngine.Localization.Components;

[System.Serializable]
public class MessageData
{
    public string senderName; // 发送者名称
    public List<string> generatedKeys = new List<string>(); // 该发送者已生成的消息key
}

[System.Serializable]
public class MessageSaveData
{
    public List<MessageData> allSenders = new List<MessageData>(); // 所有发送者的消息key集合
}

public class MessageSender : MonoBehaviour
{
    public string senderName; // 当前消息发送者名称

    [Header("TextBox Prefabs")]
    public GameObject AuthorTextBox; // 作者消息文本框预制体
    public GameObject PlayerAuthorBox; // 玩家消息文本框预制体
    public GameObject ChooseMessagePrefab; // 选项消息预制体

    [Header("Avatar Prefabs")]
    public GameObject AuthorAvatarPrefab; // 作者头像预制体
    public GameObject PlayerAvatarPrefab; // 玩家头像预制体

    [Header("Message List")]
    public List<string> messageKeys = new List<string>(); // 当前对话所有消息key

    public ScrollRect scroll; // 消息滚动区域

    private HashSet<string> generatedKeys = new HashSet<string>(); // 已生成消息key集合
    private int currentIndex = 0; // 当前消息索引
    private CustomVerticalLayout layout; // 自定义垂直布局组件
    private string SaveFolder; // 存档文件夹路径
    private string SaveFilePath; // 存档文件路径
    private bool generatedCheck = false; // 检查是否已生成消息
    private bool isChoosing = false; // 是否处于选项选择状态

    private void Awake()
    {
        // 初始化存档路径
        SaveFolder = Path.Combine(Application.persistentDataPath, "data");//持久化路径文件夹
        SaveFilePath = Path.Combine(SaveFolder, "messages.json");//存档文件路径
    }

    private void Start()
    {
        // 获取自定义布局组件
        layout = GetComponent<CustomVerticalLayout>();
        // 加载本地已生成消息key
        LoadGeneratedKeys();
        // 设置已生成消息检查标志
        generatedCheck = true;
    }

    /// <summary>
    /// 创建消息（重载，默认forceCreate为false）
    /// </summary>
    public void CreateMessage(string localizationKey) => CreateMessage(localizationKey, false);

    /// <summary>
    /// 创建消息UI，处理分支、头像、动画、存档等
    /// </summary>
    public void CreateMessage(string localizationKey, bool forceCreate)
    {
        // 1. 参数和状态检查
        if (string.IsNullOrEmpty(localizationKey)) return; // key为空直接返回
        if (!forceCreate && isChoosing) return; // 如果不是强制生成且当前处于选项选择状态，返回

        // 检查是否已生成过该消息，防止重复生成
        if (!forceCreate && generatedCheck && currentIndex < generatedKeys.Count)//非强制生成，已生成过，当前标记小于已生成列表数
        {
            currentIndex++;//转到下一句
            return;
        }

        // 如果该key已生成过，跳过
        if (!forceCreate && generatedKeys.Contains(localizationKey))
        {
            Debug.Log($"Key '{localizationKey}' already generated. Skipping.");
            return;
        }

        // 2. 分支消息处理
        string prefix = localizationKey.Substring(0, 2); // 获取key前缀判断消息类型

        // 处理选择分支（如“是/否”选项）
        if (prefix == "PC")
        {
            string id = localizationKey.Substring(3); // 获取分支id
            CreateChooseMessage($"PM_{id}_Y", true);  // 生成“是”选项
            CreateChooseMessage($"PM_{id}_N", false); // 生成“否”选项
            return;
        }

        // 处理AC类型key，根据上一次选择结果生成AM key
        if (prefix == "AC")
        {
            localizationKey = ResolveACKey(localizationKey);
            if (localizationKey == null)
            {
                return;
            }
            prefix = "AM"; // 替换为作者消息前缀
        }

        // 3. 头像切换：如果说话人变了，生成对应头像
        string prevPrefix = generatedKeys.LastOrDefault()?.Substring(0, 2);
        if (prevPrefix != prefix)
        {
            var avatarPrefab = prefix == "AM" ? AuthorAvatarPrefab : PlayerAvatarPrefab;
            if (avatarPrefab != null)
            {
                var avatar = Instantiate(avatarPrefab, transform);
                var rect = avatar.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(prefix == "AM" ? 16f : -16f, rect.anchoredPosition.y); // 左右区分
            }
        }

        // 4. 消息UI生成：根据说话人类型选择预制体
        GameObject prefab = prefix == "AM" ? AuthorTextBox : PlayerAuthorBox;
        if (prefab == null) return;

        GameObject msg = Instantiate(prefab, transform); // 实例化消息UI
        msg.transform.localPosition = Vector3.zero;

        // 设置本地化内容
        var localized = msg.GetComponent<LocalizeStringEvent>();
        if (localized != null)
        {
            localized.StringReference.TableReference = prefix == "PM" ? "PlayerMessageTable" : "AuthorMessageTable";
            localized.StringReference.TableEntryReference = localizationKey;
            localized.RefreshString(); // 刷新显示内容
        }

        // 调整文本框大小，播放动画
        var box = msg.GetComponent<CustomTextBox>();
        box?.ForceRefreshSize();
        if (generatedCheck) box?.PlayShowAnimation();

        // 5. 刷新布局，保证UI整齐
        layout.RefreshChildren();
        layout.RefreshAllTextBoxWidths();
        layout.UpdateLayout();

        // 6. 消息记录与存档
        if (prefix == "AM" || prefix == "PM")
        {
            generatedKeys.Add(localizationKey); // 记录已生成key
            if (!forceCreate)
            {
                currentIndex++;
                SaveGeneratedKeys(); // 保存到本地存档
            }
        }
        // 滚动条自动滚动到底部
        if (scroll != null) scroll.verticalNormalizedPosition = 0;
    }

    /// <summary>
    /// 解析AC类型key，根据上一次选择结果生成AM key
    /// </summary>
    private string ResolveACKey(string acKey)
    {
        string targetId = acKey.Substring(3);

        var lastPM = generatedKeys.LastOrDefault(k => k.StartsWith("PM_"));
        if (string.IsNullOrEmpty(lastPM)) return null;

        if (!lastPM.EndsWith("_Y") && !lastPM.EndsWith("_N")) return null;
        string choice = lastPM.EndsWith("_Y") ? "Y" : "N";

        return $"AM_{targetId}_{choice}";
    }

    /// <summary>
    /// 创建选项消息（如“是/否”），并设置按钮事件
    /// </summary>
    private void CreateChooseMessage(string key, bool isYes)
    {
        GameObject choose = Instantiate(ChooseMessagePrefab, transform);
        choose.transform.localPosition = Vector3.zero;

        var localized = choose.GetComponent<LocalizeStringEvent>();
        if (localized != null)
        {
            localized.StringReference.TableReference = "PlayerMessageTable";
            localized.StringReference.TableEntryReference = key;
            localized.RefreshString();
        }

        CustomTextBox box = choose.GetComponent<CustomTextBox>();
        box.ForceRefreshSize();
        layout.RefreshChildren();
        layout.RefreshAllTextBoxWidths();
        layout.UpdateLayout();

        box.PlayShowAnimation();

        var panel = choose.transform.Find("Panel");
        if (panel)
        {
            var img = panel.GetComponent<Image>();
            if (img)
                img.color = isYes ? new Color(0, 1, 0, 0.39f) : new Color(1, 0, 0, 0.39f);
        }

        var btn = choose.GetComponent<Button>();
        if (btn)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                StartCoroutine(DelayedChoose(key, isYes));
            });
        }

        isChoosing = true;
        if (scroll != null) scroll.verticalNormalizedPosition = 0;
    }

    /// <summary>
    /// 延迟处理选项选择，销毁所有选项消息后生成新消息
    /// </summary>
    private IEnumerator DelayedChoose(string key, bool isYes)
    {
        DestroyAllChooseMessages();
        isChoosing = false;
        yield return null;
        CreateMessage(key);
        layout.RefreshChildren();
        layout.RefreshAllTextBoxWidths();
        layout.UpdateLayout();
    }

    /// <summary>
    /// 销毁所有选项按钮
    /// </summary>
    private void DestroyAllChooseMessages()
    {
        foreach (var btn in GetComponentsInChildren<Button>())
        {
            Destroy(btn.gameObject);
        }
    }

    /// <summary>
    /// 从本地存档加载已生成消息key，并还原UI
    /// </summary>
    private void LoadGeneratedKeys()
    {
        var saveData = LoadJson();
        var senderData = saveData.allSenders.Find(d => d.senderName == senderName);
        if (senderData == null) return;

        generatedKeys = new HashSet<string>(senderData.generatedKeys);
        string prevPrefix = "";

        foreach (var key in senderData.generatedKeys)
        {
            string prefix = key.Substring(0, 2);
            if (prefix != "AM" && prefix != "PM") continue;

            GameObject prefab = prefix == "AM" ? AuthorTextBox : PlayerAuthorBox;
            GameObject avatarPrefab = prefix == "AM" ? AuthorAvatarPrefab : PlayerAvatarPrefab;
            if (prefab == null || avatarPrefab == null) continue;
            if (prevPrefix != prefix)
            {
                var avatar = Instantiate(avatarPrefab, transform);
                var rect = avatar.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(prefix == "AM" ? 16f : -16f, rect.anchoredPosition.y);
            }

            GameObject msg = Instantiate(prefab, transform);
            msg.transform.localPosition = Vector3.zero;

            var localized = msg.GetComponent<LocalizeStringEvent>();
            if (localized != null)
            {
                localized.StringReference.TableReference = prefix == "PM" ? "PlayerMessageTable" : "AuthorMessageTable";
                localized.StringReference.TableEntryReference = key;
                localized.RefreshString();
            }

            var box = msg.GetComponent<CustomTextBox>();
            box?.ForceRefreshSize();

            prevPrefix = prefix;
        }

        currentIndex = generatedKeys.Count;
        Debug.Log(currentIndex);
        if (scroll != null) scroll.verticalNormalizedPosition = 0;
    }

    /// <summary>
    /// 保存当前已生成消息key到本地
    /// </summary>
    private void SaveGeneratedKeys()
    {
        if (!Directory.Exists(SaveFolder))
            Directory.CreateDirectory(SaveFolder);

        var saveData = LoadJson();
        var senderData = saveData.allSenders.Find(d => d.senderName == senderName);
        if (senderData == null)
        {
            senderData = new MessageData { senderName = senderName };
            saveData.allSenders.Add(senderData);
        }

        senderData.generatedKeys = generatedKeys.ToList();
        File.WriteAllText(SaveFilePath, JsonUtility.ToJson(saveData, true));
    }

    /// <summary>
    /// 读取本地存档
    /// </summary>
    private MessageSaveData LoadJson()
    {
        if (File.Exists(SaveFilePath))
        {
            string json = File.ReadAllText(SaveFilePath);
            return JsonUtility.FromJson<MessageSaveData>(json);
        }
        return new MessageSaveData();
    }

    /// <summary>
    /// 重置所有消息和UI，并清除本地存档
    /// </summary>
    public void ResetAll()
    {
        currentIndex = 0;
        generatedKeys.Clear();
        isChoosing = false;

        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        DeleteSenderFromJson();
    }

    /// <summary>
    /// 从本地存档中删除当前发送者数据
    /// </summary>
    private void DeleteSenderFromJson()
    {
        var saveData = LoadJson();
        saveData.allSenders.RemoveAll(d => d.senderName == senderName);
        File.WriteAllText(SaveFilePath, JsonUtility.ToJson(saveData, true));
    }

    /// <summary>
    /// 按索引生成消息
    /// </summary>
    public void CreateMessageByList(int idx)
    {
        if (idx < 0 || idx >= messageKeys.Count) return;
        CreateMessage(messageKeys[idx]);
    }

    /// <summary>
    /// 顺序生成下一个消息
    /// </summary>
    public void CreateMessageNext()
    {
        if (isChoosing || currentIndex >= messageKeys.Count) return;
        CreateMessage(messageKeys[currentIndex]);
    }

    /// <summary>
    /// 一次性生成所有消息
    /// </summary>
    public void CreateMessageAllList()
    {
        while (currentIndex < messageKeys.Count && !isChoosing)
            CreateMessage(messageKeys[currentIndex]);
    }
}
