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
    [SerializeField] private Text inputFieldText;

    [SerializeField] private HoverableButton submitButton;

    [SerializeField] private RectTransform optionLayout;
    [SerializeField] private CanvasGroup optionLayoutCanvasGroup;
    [SerializeField] private GameObject optionPrefab;

    private ChatTipGroup chatTipGroup;
    
    private Sequence seq;

    private bool optionSubmitted = true;

    protected override void Awake()
    {
        base.Awake();
        chatTipGroup = FindObjectOfType<ChatTipGroup>();
    }

    protected override void Init()
    {
        inputFieldText.text = "";

        //点击输入区域
        inputFieldButton.onClick.AddListener(ShowDialogueOptions);

        // 点击发送消息
        submitButton.onClick.AddListener(Submit);

        ChatManager.Instance.chatWindow = this;
        StartCoroutine(WaitToTriggerInit(3f));
    }

    public IEnumerator  WaitToTriggerInit(float time)
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
                inputFieldText.text = outputport.name;
                //ChatManager.Instance.ChoosedChatData = outputport.name;
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
        if (optionLayoutCanvasGroup.interactable) return;

        if (seq != null && seq.IsActive()) return;

        chatScrollViewRect.sizeDelta = new Vector2(chatScrollViewRect.sizeDelta.x, chatScrollViewRect.sizeDelta.y - optionLayout.sizeDelta.y + 2);

        ResetScroll();

        seq = DOTween.Sequence();

        seq.Join(typeArea.DOSizeDelta(new Vector2(typeArea.sizeDelta.x, (inputFieldButton.transform as RectTransform).sizeDelta.y + optionLayout.sizeDelta.y - 2), optionAnimDuration))
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
           .Join(typeArea.DOSizeDelta(new Vector2(typeArea.sizeDelta.x, (inputFieldButton.transform as RectTransform).sizeDelta.y), optionAnimDuration));
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
        if (string.IsNullOrEmpty(inputFieldText.text)) return;
        InterruptChoose();
        ChatManager.Instance.Submit();
    }

    public void InterruptChoose()
    {
        inputFieldText.text = "";
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
}