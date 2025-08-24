using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private GameObject innerContentsMask; // 用来限制放置和取出内容物

    [SerializeField] private GameObject eventButtonPrefab;

    [SerializeField] private RectTransform selectRect; // 选择框

    private string currentDisplay;

    private Card currentDisplayedCard;
    private Bag innerBag;
    private bool onlyDetails;

    protected override void Awake()
    {
        base.Awake();
        EventManager.Instance.AddListener<Card>(EventType.ChangeCardProperty, RefreshCard);
        EventManager.Instance.AddListener<EnvironmentBag>(EventType.Move, OnMove);
        EventManager.Instance.AddListener<ChangePlayerBagCardsArgs>(EventType.ChangePlayerBagCards, OnPlayerCardsChanged);
    }

    private void OnDestroy()
    {
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
            if (!onlyDetails)
            {
                DisplayEventButtons();
                if (currentDisplay == "内容物" && innerBag != null)
                    DisplayBag(innerBag);
            }
        }
    }

    bool moved = false;
    private void OnMove(EnvironmentBag curEnvironmentBag)
    {
        // 切地点时清除显示
        moved = true;
    }

    public void Display(SlotCards slotCards, bool onlyDetails = false)
    {
        // 清除原数据
        Clear();

        if (slotCards.IsEmpty) return;

        // 记录当前显示的卡牌
        currentDisplayedCard = slotCards.PeekCard();

        currentDisplayedCard.Transform = slot.transform;

        Display(onlyDetails);
    }

    public void Display(Card card, bool onlyDetails = false)
    {
        // 清除原数据
        Clear();

        if (card == null) return;

        // 记录当前显示的卡牌
        currentDisplayedCard = card;

        currentDisplayedCard.Transform = slot.transform;

        Display(onlyDetails);
    }

    private void Display(bool onlyDetails = false)
    {
        this.onlyDetails = onlyDetails;

        // 显示卡牌
        slot.DisplayCard(currentDisplayedCard, 1, false);

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

                innerContentsMask.SetActive(!component.canAddOrRemove);
            }
            else
            {
                innerContentsButton.gameObject.SetActive(false);
            }
        }

        // 显示详情
        DisplayDetails();

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
        if (currentDisplayedCard == null) return;

        ObjectBufferPool.Instance.RestoreAllChildren(buttonLayout);

        HoverableButton button;
        Text btnText;
        bool interactable;
        foreach (var e in currentDisplayedCard.Events)
        {
            var card = currentDisplayedCard;
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
                    if (moved) Clear();
                    // 否则刷新卡牌和详情
                    else card?.RefreshSlot();

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

        if (currentDisplayedCard != null)
            currentDisplayedCard.Transform = null;

        onlyDetails = false;
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
