using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatWindow : WindowBase, IPointerDownHandler
{
    [SerializeField] private GameObject playerChatPrefab;
    [SerializeField] private GameObject othersChatPrefab;
    [SerializeField] private GameObject narrtionPrefab;

    [SerializeField] private ChatLayoutGroup chatLayoutGroup;
    [SerializeField] private RectTransform chatScrollViewRect;

    [SerializeField] private RectTransform typeArea;

    [SerializeField] private Button inputFieldButton;
    [SerializeField] private InputField inputField;
    [SerializeField] private Text inputFieldText;

    [SerializeField] private HoverableButton submitButton;
    [Header("LLM Idle Chat")]
    [SerializeField] private bool enableLLMInIdle = true;
    [SerializeField] private OpenAICompatibleLLMClient llmClient;
    [SerializeField] private string llmSystemPrompt = "You are a helpful assistant in this game world.";

    [SerializeField] private RectTransform optionLayout;
    [SerializeField] private CanvasGroup optionLayoutCanvasGroup;
    [SerializeField] private GameObject optionPrefab;

    private ChatTipGroup chatTipGroup;
    
    private Sequence seq;

    private bool optionSubmitted = true;
    private bool llmRequesting = false;

    protected override void Awake()
    {
        base.Awake();
        chatTipGroup = FindObjectOfType<ChatTipGroup>();
    }

    protected override void Init()
    {
        SetDisplayedInputText(string.Empty);

        // 点击输入区域（旧方案：Button）
        if (inputFieldButton != null)
            inputFieldButton.onClick.AddListener(TryShowDialogueOptions);

        // 选中输入框（新方案：InputField，不需要同物体再挂 Button）
        if (inputField != null)
            BindInputFieldSelectTrigger(inputField);

        // 点击发送消息
        submitButton.onClick.AddListener(Submit);

        // 如果未主动打开窗口，3s后自动开始剧情
        StartCoroutine(WaitToTriggerInit(3f));
    }

    public IEnumerator WaitToTriggerInit(float time)
    {
        yield return new WaitForSeconds(time);
        if (!init)
        {
            init = true;
            //生成已发送过的对话数据
            ChatManager.Instance.InitChat();
            ResetScroll();
        }
    }

    public void RemoveFirstMessage()
    {
        var first = chatLayoutGroup.transform.GetChild(0);
        ObjectBufferPool.Instance.Restore(first.gameObject);
        // 更新组件高度
        MonoUtility.UpdateChatLayoutSize(chatLayoutGroup);
        ResetScroll();
    }

    /// <summary>
    /// 添加一条对话
    /// </summary>
    /// <param name="sender"></param>
    public void CreateMessage(MessageSenderEnum sender, string content, bool addChatTip = true)
    {
        GameObject prefab = null;
        switch (sender)
        {
            case MessageSenderEnum.NPC:
                prefab = othersChatPrefab;
                break;
            case MessageSenderEnum.Player:
                prefab = playerChatPrefab;
                break;
            case MessageSenderEnum.Aside:
                prefab = narrtionPrefab;
                break;
            case MessageSenderEnum.Alert:   
                prefab = narrtionPrefab;
                break;
        }

        // 创建聊天气泡
        ObjectBufferPool.Instance.Get(prefab, chatLayoutGroup.transform).GetComponent<CustomTextBox>().SetText(content);

        // 更新组件高度
        MonoUtility.UpdateChatLayoutSize(chatLayoutGroup);

        ResetScroll();

        if (addChatTip && !focused) CreateChatTip(sender, content, 10f);
    }

    private void ResetScroll()
    {
        // 设置滚动到底部
        Canvas.ForceUpdateCanvases();
        chatScrollViewRect.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
    }

    /// <summary>
    /// 设置对话选项
    /// </summary>
    public void SetDialogueOptions(GraphData.SerializedNode nodeData)
    {
        ObjectBufferPool.Instance.RestoreAllChildren(optionLayout);

        GameObject obj;
        HoverableButton button;
        foreach (var outputport in nodeData.outputports)
        {
            obj = ObjectBufferPool.Instance.Get(optionPrefab, optionLayout);
            obj.GetComponent<CustomTextBox>().SetText(outputport.name);

            button = obj.GetComponent<HoverableButton>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                SetDisplayedInputText(outputport.name);
                ChatManager.Instance.ChoosedChatData = outputport.name;
            });

            obj.transform.SetAsLastSibling();
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(optionLayout);

        optionSubmitted = false;

        ShowDialogueOptions();

        timer = 0;
    }

    float optionAnimDuration = 0.15f;
    /// <summary>
    /// 显示对话选项
    /// </summary>
    public void ShowDialogueOptions()
    {
        if (!CanShowDialogueOptions()) return;

        if (optionLayoutCanvasGroup.interactable) return;

        if (seq != null && seq.IsActive()) return;

        chatScrollViewRect.sizeDelta = new Vector2(chatScrollViewRect.sizeDelta.x, chatScrollViewRect.sizeDelta.y - optionLayout.sizeDelta.y + 2);

        ResetScroll();

        seq = DOTween.Sequence();

        float inputAreaHeight = GetInputAreaHeight();
        seq.Join(typeArea.DOSizeDelta(new Vector2(typeArea.sizeDelta.x, inputAreaHeight + optionLayout.sizeDelta.y - 2), optionAnimDuration))
           .OnComplete(() =>
           {
               optionLayoutCanvasGroup.alpha = 1f;
               optionLayoutCanvasGroup.blocksRaycasts = optionLayoutCanvasGroup.interactable = true;
           });
    }

    /// <summary>
    /// 隐藏对话选项
    /// </summary>
    public void HideDialogueOptions()
    {
        if (!optionSubmitted) return;

        if (!optionLayoutCanvasGroup.interactable) return;

        if (seq != null && seq.IsActive()) return;

        seq = DOTween.Sequence();

        seq.OnStart(() =>
            {
                optionLayoutCanvasGroup.alpha = 0f;
                optionLayoutCanvasGroup.blocksRaycasts = optionLayoutCanvasGroup.interactable = false;
            })
           .Join(chatScrollViewRect.DOSizeDelta(new Vector2(chatScrollViewRect.sizeDelta.x, chatScrollViewRect.sizeDelta.y + optionLayout.sizeDelta.y - 2), optionAnimDuration))
           .Join(typeArea.DOSizeDelta(new Vector2(typeArea.sizeDelta.x, GetInputAreaHeight()), optionAnimDuration));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 点击到聊天区域，隐藏选项
        var currentObject = eventData.pointerCurrentRaycast.gameObject;
        if (currentObject.name == chatScrollViewRect.gameObject.name)
        {
            // 隐藏选项
            HideDialogueOptions();
        }
    }

    private void Submit()
    {
        string text = GetDisplayedInputText();
        if (string.IsNullOrWhiteSpace(text)) return;

        if (ChatManager.Instance != null && ChatManager.Instance.Choosing)
        {
            // 剧情节点推进：只有在选项与节点状态都有效时才提交
            if (CanSubmitStoryOption())
            {
                InterruptChoose();
                ChatManager.Instance.Submit();
                return;
            }

            // 进入了选择态但没有有效选项，直接退出选择态，避免空引用链路
            ChatManager.Instance.Choosing = false;
            ChatManager.Instance.ChoosedChatData = null;
        }

        // 空闲状态下接入大模型
        SubmitToLLM(text);
    }

    public void InterruptChoose()
    {
        SetDisplayedInputText(string.Empty);
        ObjectBufferPool.Instance.RestoreAllChildren(optionLayout);
        optionSubmitted = true;
        HideDialogueOptions();
        timer = int.MaxValue;
    }

    private void CreateChatTip(MessageSenderEnum sender, string text, float lifeTime)
    {
        chatTipGroup.AddTip(sender, text, lifeTime);
    }

    private bool init = false;
    protected override void OnFocused()
    {
        if (!init)
        {
            init = true;
            //生成已发送过的对话数据
            ChatManager.Instance.InitChat();
            ResetScroll();
        }
        chatTipGroup.Clear();
    }

    private float timer = int.MaxValue;
    private float alertTimeInterval = 10f;
    private void Update()
    {
        if (timer < alertTimeInterval)
        {
            timer += Time.deltaTime;
            if (timer >= alertTimeInterval)
            {
                if (!focused) CreateChatTip(MessageSenderEnum.Alert, "您有一条待发送消息", int.MaxValue);
            }
        }
    }

    private void TryShowDialogueOptions()
    {
        if (!CanShowDialogueOptions()) return;
        ShowDialogueOptions();
    }

    private bool CanShowDialogueOptions()
    {
        return !optionSubmitted && ChatManager.Instance != null && ChatManager.Instance.Choosing;
    }

    private float GetInputAreaHeight()
    {
        if (inputFieldButton != null)
            return (inputFieldButton.transform as RectTransform).sizeDelta.y;

        if (inputField != null)
            return (inputField.transform as RectTransform).sizeDelta.y;

        if (inputFieldText != null)
            return (inputFieldText.transform as RectTransform).sizeDelta.y;

        return 0f;
    }

    private string GetDisplayedInputText()
    {
        if (inputField != null) return inputField.text;
        if (inputFieldText != null) return inputFieldText.text;
        return string.Empty;
    }

    private void SetDisplayedInputText(string value)
    {
        if (inputField != null) inputField.text = value;
        if (inputFieldText != null) inputFieldText.text = value;
    }

    private void SubmitToLLM(string userText)
    {
        if (!enableLLMInIdle) return;
        if (llmRequesting) return;

        if (llmClient == null)
        {
            CreateMessage(MessageSenderEnum.Alert, "LLM 未配置：请在 ChatWindow 上绑定 OpenAICompatibleLLMClient。");
            return;
        }

        llmRequesting = true;
        SetSubmitInteractable(false);

        // 先显示玩家输入
        CreateMessage(MessageSenderEnum.Player, userText);
        SetDisplayedInputText(string.Empty);

        llmClient.SendUserMessage(
            llmSystemPrompt,
            userText,
            onSuccess: reply =>
            {
                CreateMessage(MessageSenderEnum.NPC, reply);
                llmRequesting = false;
                SetSubmitInteractable(true);
            },
            onError: error =>
            {
                CreateMessage(MessageSenderEnum.Alert, "LLM 请求失败: " + error);
                llmRequesting = false;
                SetSubmitInteractable(true);
            }
        );
    }

    private void SetSubmitInteractable(bool interactable)
    {
        if (submitButton != null)
            submitButton.Interactable = interactable;
    }

    private bool CanSubmitStoryOption()
    {
        if (ChatManager.Instance == null) return false;
        if (string.IsNullOrEmpty(ChatManager.Instance.ChoosedChatData)) return false;

        var reader = ReadChatParagraph.Instance;
        if (reader == null) return false;
        if (reader.CurGraphData == null || reader.CurNode == null) return false;

        return true;
    }

    private void BindInputFieldSelectTrigger(InputField targetInputField)
    {
        var trigger = targetInputField.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = targetInputField.gameObject.AddComponent<EventTrigger>();

        AddEventTriggerListener(trigger, EventTriggerType.Select, _ => TryShowDialogueOptions());
        AddEventTriggerListener(trigger, EventTriggerType.PointerClick, _ => TryShowDialogueOptions());
    }

    private void AddEventTriggerListener(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        var entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }
}
