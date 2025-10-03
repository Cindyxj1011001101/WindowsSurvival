using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public enum DisplayType
{
    All,
    OnlyDetails,
    DetailsAndCraftButton
}

public class DetailsWindow : BagWindow
{
    [SerializeField] private Text detailsText;
    [SerializeField] private GameObject detailsScrollView;
    [SerializeField] private Transform buttonLayout;
    [SerializeField] private CardSlot slot;
    [SerializeField] private RectTransform contentsView;

    [SerializeField] private Transform menuLayout; // 菜单布局
    [SerializeField] private HoverableButton detailsButton; // 显示详细信息按钮
    [SerializeField] private HoverableButton innerContentsButton; // 显示内部内容按钮

    [SerializeField] private GameObject eventButtonPrefab;

    [SerializeField] private RectTransform selectRect; // 选择框

    private string currentDisplay;

    private Card currentDisplayedCard;
    private Bag innerBag;
    private DisplayType displayType = DisplayType.All;

    protected override void Awake()
    {
        base.Awake();
        EventManager.Instance.AddListener<Card>(EventType.ChangeCardProperty, RefreshCard);
        EventManager.Instance.AddListener<EnvironmentBag>(EventType.ChangeEnv, OnMove);
        EventManager.Instance.AddListener<ChangePlayerBagCardsArgs>(EventType.ChangePlayerBagCards, OnPlayerCardsChanged);
    }

    private void OnDestroy()
    {
        Clear();
        EventManager.Instance.RemoveListener<Card>(EventType.ChangeCardProperty, RefreshCard);
        EventManager.Instance.RemoveListener<EnvironmentBag>(EventType.ChangeEnv, OnMove);
        EventManager.Instance.RemoveListener<ChangePlayerBagCardsArgs>(EventType.ChangePlayerBagCards, OnPlayerCardsChanged);
    }

    protected override void Init()
    {
        if (currentDisplayedCard == null)
        {
            Clear();
        }

        detailsButton.onClick.AddListener(() =>
        {
            if (currentDisplayedCard != null && currentDisplay != "详情")
            {
                DisplayDetails();
            }
        });

        innerContentsButton.onClick.AddListener(() =>
        {
            if (currentDisplayedCard != null && currentDisplay != "内容物")
            {
                DisplayInnerContents();
            }
        });
    }

    /// <summary>
    /// 当玩家背包物品变化时触发，这是为了刷新卡牌事件的触发条件
    /// </summary>
    /// <param name="args"></param>
    private void OnPlayerCardsChanged(ChangePlayerBagCardsArgs args)
    {
        if (currentDisplayedCard != null)
            DisplayEventButtons();
    }

    private void RefreshCard(Card card)
    {
        if (currentDisplayedCard != card) return;

        // 如果卡牌要被销毁
        if (currentDisplayedCard.Destroyed)
        {
            // 尝试从这个卡牌的slotCount里取出同类卡牌并刷新
            if (currentDisplayedCard.SlotCards.ContainsByCardId(currentDisplayedCard.CardId))
                Display(currentDisplayedCard.SlotCards);
            // 否则清空显示
            else
                Clear();
        }
        // 正常刷新显示
        else
        {
            slot.DisplayCard(currentDisplayedCard, 1, false);
            DisplayEventButtons();
            //if (currentDisplay == "内容物" && innerBag != null)
            //    DisplayBag(innerBag);
        }
    }

    bool moved = false;
    private void OnMove(EnvironmentBag curEnvironmentBag)
    {
        // 切地点时清除显示
        moved = true;
    }

    public void Display(SlotCards slotCards, DisplayType displayType = DisplayType.All)
    {
        // 清除原数据
        Clear();

        if (slotCards.IsEmpty) return;

        // 记录当前显示的卡牌
        currentDisplayedCard = slotCards.PeekCard();

        currentDisplayedCard.Transform = slot.transform;

        Display(displayType);
    }

    public void Display(Card card, DisplayType displayType = DisplayType.All)
    {
        // 清除原数据
        Clear();

        if (card == null) return;

        // 记录当前显示的卡牌
        currentDisplayedCard = card;

        currentDisplayedCard.Transform = slot.transform;

        Display(displayType);
    }

    private void Display(DisplayType displayType = DisplayType.All)
    {
        this.displayType = displayType;

        // 显示卡牌
        slot.DisplayCard(currentDisplayedCard, 1, false);

        EventManager.Instance.TriggerEvent(EventType.DialogueCondition, new SubscribeActionArgs("Detail", currentDisplayedCard.CardName));

        // 显示可交互按钮
        DisplayEventButtons();

        switch (displayType)
        {
            case DisplayType.All:
                // 有内容物优先显示内容物
                if (currentDisplayedCard.TryGetComponent<InnerContentsComponent>(out var component) && component.display)
                {
                    innerContentsButton.gameObject.SetActive(true);
                    innerContentsButton.Interactable = true;
                    innerBag = component.bag;

                    // 显示内容物
                    DisplayInnerContents();
                }
                // 优先显示详情
                else
                {
                    innerContentsButton.gameObject.SetActive(false);
                    innerContentsButton.Interactable = false;
                    // 显示详情
                    DisplayDetails();
                }
                break;
            case DisplayType.OnlyDetails:
            case DisplayType.DetailsAndCraftButton:
            default:
                DisplayDetails();
                break;
        }

        // 打开详情如果卡牌有循环音
        if (currentDisplayedCard.HasLoopSound)
            currentDisplayedCard.OnDetailOpen();
    }

    private void DisplayDetails()
    {
        currentDisplay = "详情";

        // 清除内容物卡牌的显示
        ClearBag();

        detailsScrollView.SetActive(true);
        contentsView.gameObject.SetActive(false);

        // 显示卡牌详细信息
        detailsText.text = currentDisplayedCard.CardDesc;

        SelectWithTween(detailsButton.GetComponent<RectTransform>());
    }

    private void DisplayInnerContents()
    {
        currentDisplay = "内容物";

        detailsScrollView.SetActive(false);
        contentsView.gameObject.SetActive(true);

        DisplayBag(innerBag);

        SelectWithTween(innerContentsButton.GetComponent<RectTransform>());
    }

    private void DisplayEventButtons()
    {
        if (currentDisplayedCard == null || displayType == DisplayType.OnlyDetails) return;

        ObjectBufferPool.Instance.RestoreAllChildren(buttonLayout);


        // 显示详情和前往制作按钮
        if (displayType == DisplayType.DetailsAndCraftButton)
        {
            var button = ObjectBufferPool.Instance.Get(eventButtonPrefab, buttonLayout).GetComponent<HoverableButton>();
            var btnText = button.GetComponentInChildren<Text>();
            btnText.text = "前往制作";
            button.Interactable = WindowsManager.Instance.GetUnlockedShortcuts().Contains("Craft");
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                var window = WindowsManager.Instance.OpenWindow("Craft") as CraftWindow;
                window.DisplayRecipe(currentDisplayedCard.CardId);
            });

            if (button.Interactable)
            {
                btnText.color = ColorManager.White;
                button.GetComponent<HoverTipController>().SetTip("");
            }
            else
            {
                btnText.color = ColorManager.DarkGrey;
                button.GetComponent<HoverTipController>().SetTip("制作窗口尚未解锁");
            }

            button.transform.localScale = Vector3.one; // 确保按钮缩放为1
            button.transform.SetAsLastSibling();
            return;
        }

        foreach (var e in currentDisplayedCard.Events)
        {
            var card = currentDisplayedCard;
            var button = ObjectBufferPool.Instance.Get(eventButtonPrefab, buttonLayout).GetComponent<HoverableButton>();
            var btnText = button.GetComponentInChildren<Text>();
            btnText.text = e.name;

            var interactable = e.Judge();
            button.Interactable = interactable;

            // 判断cardEvent是否满足条件
            if (interactable)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    // 先执行事件
                    e.Inovke(out string tip);

                    // 显示提示
                    button.transform.ShowTip(tip, 1.4f);

                    // 显示状态变化
                    var playerStateChanges = e.GetPlayerEffects();
                    if (!playerStateChanges.IsNullOrEmpty())
                    {
                        foreach (var (state, delta) in playerStateChanges)
                        {
                            ShowStateChange(state, delta, button.transform.position);
                        }
                    }

                    var envStateChanges = e.GetEnvEffects();
                    if (!envStateChanges.IsNullOrEmpty())
                    {
                        foreach (var (state, delta) in envStateChanges)
                        {
                            ShowStateChange(state, delta, button.transform.position);
                        }
                    }

                    // 改变场景了就清空详情
                    if (moved) Clear();
                    // 否则刷新卡牌和详情
                    else RefreshCard(card);
                    //else card?.RefreshSlot();

                    // 否则尝试刷新
                    //else if (currentDisplayedCard != null && !currentDisplayedCard.Destroyed)
                    //    DisplayCardDetails(currentDisplayedCard);
                });

                // 设置提示
                button.GetComponent<HoverTipController>().SetTip(e.Description, e.GetTimeEffect(), e.GetPlayerEffects(), e.GetEnvEffects());
            }
            else
            {
                btnText.color = ColorManager.DarkGrey;
                button.GetComponent<HoverTipController>().SetTip(e.Description);
            }

            button.transform.localScale = Vector3.one; // 确保按钮缩放为1
            button.transform.SetAsLastSibling();
        }

        MonoUtility.UpdateLayoutSize(buttonLayout.GetComponent<ILayoutGroup>());
    }

    public void Clear()
    {
        currentDisplay = null;

        ClearBag();

        moved = false;
        slot.Clear();

        // 关闭时如果卡牌有循环音将循环音减小
        if (currentDisplayedCard != null && currentDisplayedCard.HasLoopSound)
            currentDisplayedCard.OnDetailClose();

        if (currentDisplayedCard != null && !currentDisplayedCard.Destroyed)
            currentDisplayedCard.Transform = null;

        displayType = DisplayType.All;
        currentDisplayedCard = null;
        innerBag = null;
        detailsText.text = "";
        contentsView.gameObject.SetActive(false);
        innerContentsButton.gameObject.SetActive(false);
        ObjectBufferPool.Instance.RestoreAllChildren(buttonLayout);
    }

    private void SelectWithTween(RectTransform target)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(menuLayout as RectTransform);

        Vector2 targetPos = new(target.anchoredPosition.x, selectRect.anchoredPosition.y);

        selectRect.DOKill();
        selectRect.DOAnchorPos(targetPos, 0.2f).SetEase(Ease.OutQuad);
    }

    public override void Hide(ShowMode showMode = ShowMode.Fade, UnityEngine.Events.UnityAction onFinished = null)
    {
        if (currentDisplayedCard != null && currentDisplayedCard.HasLoopSound)
            currentDisplayedCard.OnDetailClose();
        base.Hide(showMode, onFinished);
    }

    public override void Minimize(Transform shortcut)
    {
        if (currentDisplayedCard != null && currentDisplayedCard.HasLoopSound)
            currentDisplayedCard.OnDetailClose();
        base.Minimize(shortcut);
    }

    /// <summary>
    /// 显示玩家状态的变化值
    /// </summary>
    /// <param name="state"></param>
    /// <param name="delta"></param>
    public void ShowStateChange(PlayerStateEnum state, float delta, Vector3 center)
    {
        var stateWindow = WindowsManager.Instance.OpenWindow("State") as StateWindow;

        ShowStateChange(stateWindow.stateSliders[state].icon, StateManager.Instance.PlayerStateDict[state].MaxValue, delta, center);
    }

    /// <summary>
    /// 显示玩家状态的变化值
    /// </summary>
    /// <param name="state"></param>
    /// <param name="delta"></param>
    public void ShowStateChange(EnvironmentStateEnum state, float delta, Vector3 center)
    {
        var envWindow = WindowsManager.Instance.OpenWindow("EnvironmentBag") as EnvironmentBagWindow;

        float maxValue;
        if (state == EnvironmentStateEnum.Electricity)
            maxValue = StateManager.Instance.Electricity.MaxValue;
        else if (state == EnvironmentStateEnum.WaterLevel)
            maxValue = StateManager.Instance.WaterLevel.MaxValue;
        else
            maxValue = GameManager.Instance.CurEnvironmentBag.StateDict[state].MaxValue;

        ShowStateChange(envWindow.continuousValueStates[state].icon, maxValue, delta, center);
    }

    /// <summary>
    /// 显示玩家状态的变化值
    /// </summary>
    /// <param name="state"></param>
    /// <param name="delta"></param>
    public void ShowStateChange(Image icon, float stateMaxValue, float delta, Vector3 center)
    {
        var stateWindow = WindowsManager.Instance.OpenWindow("State") as StateWindow;

        float halfLength = 85f;
        float xMax = center.x + halfLength;
        float xMin = center.x - halfLength;
        float yMax = center.y + halfLength;
        float yMin = center.y - halfLength;

        Vector3 randomPos;
        Vector3 targetPos;

        var count = 2 + Mathf.CeilToInt(Mathf.Abs(delta) * 10 / stateMaxValue);

        for (int i = 0; i < count; i++)
        {
            randomPos = new(Random.Range(xMin, xMax), Random.Range(yMin, yMax));
            var transform = ObjectBufferPool.Instance.Get("Prefabs/UI/Controls/State", "StateIcon", WindowsManager.Instance.FloatingTipLayer).transform;

            transform.GetComponent<Image>().sprite = icon.sprite;

            var seq = DOTween.Sequence();

            transform.position = randomPos;
            transform.localScale = Vector3.one * .8f;
            seq.Append(transform.DOScale(1.2f, .3f));

            seq.AppendInterval(.4f);

            targetPos = icon.transform.position;
            seq.Join(transform.DOMove(targetPos, .6f));
            seq.Join(transform.DOScale(.8f, .6f));

            seq.OnComplete(() =>
            {
                ObjectBufferPool.Instance.Restore(transform.gameObject);
                icon.transform.DOScale(1.3f, .15f).SetLoops(2, LoopType.Yoyo);
            }); // 总时长1.3s
        }
    }
}
