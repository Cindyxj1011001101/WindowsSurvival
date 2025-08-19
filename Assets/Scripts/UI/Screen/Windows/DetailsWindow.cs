using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DetailsWindow : BagWindow
{
    [SerializeField] private Text detailsText;
    [SerializeField] private Transform buttonLayout;
    [SerializeField] private CardSlot slot;
    [SerializeField] private RectTransform contentsView;

    [SerializeField] private Transform menuLayout; // 菜单布局
    [SerializeField] private HoverableButton detailsButton; // 显示详细信息按钮
    [SerializeField] private HoverableButton innerContentsButton; // 显示内部内容按钮

    [SerializeField] private GameObject eventButtonPrefab;

    [SerializeField] private RectTransform selectRect; // 选择框

    private Card currentDisplayedCard;
    private Bag innerBag;

    protected override void Awake()
    {
        base.Awake();
        EventManager.Instance.AddListener(EventType.ChangeCardProperty, RefreshCard);
        EventManager.Instance.AddListener<Card>(EventType.ChangeCardProperty, RefreshCard);
        EventManager.Instance.AddListener<EnvironmentBag>(EventType.Move, OnMove);
        EventManager.Instance.AddListener<ChangePlayerBagCardsArgs>(EventType.ChangePlayerBagCards, OnPlayerCardsChanged);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EventType.ChangeCardProperty, RefreshCard);
        EventManager.Instance.RemoveListener<Card>(EventType.ChangeCardProperty, RefreshCard);
        EventManager.Instance.RemoveListener<EnvironmentBag>(EventType.Move, OnMove);
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
            if (currentDisplayedCard != null)
            {
                DisplayDetails();
            }
        });

        innerContentsButton.onClick.AddListener(() =>
        {
            if (currentDisplayedCard != null)
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

    public void RefreshCard()
    {
        if (currentDisplayedCard != null)
            DisplayCard();
    }

    private void RefreshCard(Card card)
    {
        if (currentDisplayedCard != card) return;

        // 如果卡牌要被销毁
        if (currentDisplayedCard.Destroyed)
        {
            // 尝试从这个卡牌的slotCount里取出同类卡牌并刷新
            if (currentDisplayedCard.SlotCards.ContainsByCardId(currentDisplayedCard.CardId))
                DisplayCardDetails(currentDisplayedCard.SlotCards);
            // 否则清空显示
            else
                Clear();
        }
        // 正常刷新显示
        else
        {
            RefreshCard();
        }
    }

    bool moved = false;
    private void OnMove(EnvironmentBag curEnvironmentBag)
    {
        // 切地点时清除显示
        moved = true;
    }

    public void DisplayCardDetails(SlotCards slotCards, bool onlyDetails = false)
    {
        // 清除原数据
        Clear();

        if (slotCards.IsEmpty) return;

        // 记录当前显示的卡牌
        currentDisplayedCard = slotCards.PeekCard();

        currentDisplayedCard.Transform = slot.transform;

        DisplayCardDetails(onlyDetails);
    }

    public void DisplayCardDetails(Card card, bool onlyDetails = false)
    {
        // 清除原数据
        Clear();

        if (card == null) return;

        // 记录当前显示的卡牌
        currentDisplayedCard = card;

        currentDisplayedCard.Transform = slot.transform;

        DisplayCardDetails(onlyDetails);
    }

    private void DisplayCardDetails(bool onlyDetails = false)
    {
        // 显示卡牌
        DisplayCard();

        // 显示详情
        DisplayDetails();

        EventManager.Instance.TriggerEvent(EventType.DialogueCondition, new SubscribeActionArgs("Detail", currentDisplayedCard.CardName));

        if (!onlyDetails)
        {
            // 显示可选择按钮
            DisplayEventButtons();

            innerContentsButton.Interactable = false;

            // 初始化内容物
            if (currentDisplayedCard.TryGetComponent<InnerContentsComponent>(out var component) && component.display)
            {
                innerContentsButton.gameObject.SetActive(true);
                innerContentsButton.Interactable = true;
                innerBag = component.bag;
            }
            else
            {
                innerContentsButton.gameObject.SetActive(false);
            }
        }

        // 打开详情如果卡牌有循环音
        if (currentDisplayedCard.HasLoopSound)
            currentDisplayedCard.OnDetailOpen();
    }

    private void DisplayCard()
    {
        slot.DisplayCard(currentDisplayedCard, 1, false);
    }

    private void DisplayDetails()
    {
        // 清除内容物卡牌的显示
        ClearBag();

        detailsText.gameObject.SetActive(true);
        contentsView.gameObject.SetActive(false);

        // 显示卡牌详细信息
        detailsText.text = currentDisplayedCard.CardDesc;

        SelectWithTween(detailsButton.GetComponent<RectTransform>());
    }

    private void DisplayInnerContents()
    {
        detailsText.gameObject.SetActive(false);
        contentsView.gameObject.SetActive(true);

        DisplayBag(innerBag);

        SelectWithTween(innerContentsButton.GetComponent<RectTransform>());
    }

    private void DisplayEventButtons()
    {
        if (currentDisplayedCard == null) return;

        ObjectBufferPool.Instance.RestoreAllChildren(buttonLayout);

        HoverableButton button;
        Text btnText;
        bool interactable;
        foreach (var e in currentDisplayedCard.Events)
        {
            button = ObjectBufferPool.Instance.Get(eventButtonPrefab, buttonLayout).GetComponent<HoverableButton>();
            btnText = button.GetComponentInChildren<Text>();
            btnText.text = e.name;

            interactable = e.Judge();
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
                    button.ShowTip(tip);

                    // 改变场景了就清空详情
                    if (moved)
                        Clear();
                    // 否则尝试刷新
                    else if (currentDisplayedCard != null && !currentDisplayedCard.Destroyed)
                        DisplayCardDetails(currentDisplayedCard);
                });
            }
            else
            {
                btnText.color = ColorManager.DarkGrey;
            }

            // 设置提示
            if (interactable)
                button.GetComponent<HoverTipController>().SetTip(e.Description, e.GetTimeEffect(), e.GetPlayerEffects(), e.GetEnvEffects());
            else
                button.GetComponent<HoverTipController>().SetTip(e.Description);

            button.transform.SetAsLastSibling();
        }
    }

    public void Clear()
    {
        ClearBag();

        moved = false;
        slot.Clear();

        // 关闭时如果卡牌有循环音将循环音减小
        if (currentDisplayedCard != null && currentDisplayedCard.HasLoopSound)
            currentDisplayedCard.OnDetailClose();

        if (currentDisplayedCard != null)
            currentDisplayedCard.Transform = null;

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
}
