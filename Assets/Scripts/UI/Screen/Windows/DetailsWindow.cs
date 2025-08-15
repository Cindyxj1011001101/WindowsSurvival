using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DetailsWindow : WindowBase
{
    [SerializeField] private Text detailsText;
    [SerializeField] private Transform buttonLayout;
    [SerializeField] private CardSlot slot;
    [SerializeField] private InnerBag innerBag;

    [SerializeField] private Transform menuLayout; // 菜单布局
    [SerializeField] private HoverableButton detailsButton; // 显示详细信息按钮
    [SerializeField] private HoverableButton innerContentsButton; // 显示内部内容按钮

    [SerializeField] private RectTransform selectRect; // 选择框

    private Card currentDisplayedCard;

    protected override void Awake()
    {
        base.Awake();
        EventManager.Instance.AddListener(EventType.ChangeCardProperty, RefreshCurrentDisplay);
        EventManager.Instance.AddListener<Card>(EventType.ChangeCardProperty, RefreshCurrentDisplay);
        EventManager.Instance.AddListener<EnvironmentBag>(EventType.Move, OnMove);
        EventManager.Instance.AddListener<ChangePlayerBagCardsArgs>(EventType.ChangePlayerBagCards, OnPlayerCardsChanged);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EventType.ChangeCardProperty, RefreshCurrentDisplay);
        EventManager.Instance.RemoveListener<Card>(EventType.ChangeCardProperty, RefreshCurrentDisplay);
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

    private void RefreshCurrentDisplay()
    {
        if (currentDisplayedCard != null)
            DisplayCard();
    }

    private void RefreshCurrentDisplay(Card card)
    {
        if (currentDisplayedCard != card) return;

        // 如果卡牌要被销毁
        if (currentDisplayedCard.Destroyed)
        {
            // 尝试从这个卡牌的slotCount里取出同类卡牌并刷新
            if (currentDisplayedCard.SlotCards.Count > 0 &&
                currentDisplayedCard.SlotCards[0].CardId == currentDisplayedCard.CardId)
            DisplayCardDetails(currentDisplayedCard.SlotCards);
            // 否则清空显示
            else
                Clear();
        }
        // 正常刷新显示
        else
        {
            RefreshCurrentDisplay();
        }
    }

    bool moved = false;
    private void OnMove(EnvironmentBag curEnvironmentBag)
    {
        // 切地点时清除显示
        moved = true;
    }

    public void DisplayCardDetails(List<Card> slotCards, bool onlyDetails = false)
    {
        // 清除原数据
        Clear();

        if (slotCards.IsNullOrEmpty()) return;

        // 记录当前显示的卡牌
        currentDisplayedCard = slotCards[0];

        currentDisplayedCard.TempSlotTransform = slot.transform;

        DisplayCardDetails(onlyDetails);
    }

    public void DisplayCardDetails(Card card, bool onlyDetails = false)
    {
        // 清除原数据
        Clear();

        if (card == null) return;

        // 记录当前显示的卡牌
        currentDisplayedCard = card;

        currentDisplayedCard.TempSlotTransform = slot.transform;

        DisplayCardDetails(onlyDetails);
    }

    private void DisplayCardDetails(bool onlyDetails = false)
    {
        // 显示卡牌
        DisplayCard();

        EventManager.Instance.TriggerEvent(EventType.DialogueCondition, new SubscribeActionArgs("Detail", currentDisplayedCard.CardName));

        if (!onlyDetails)
        {
            // 显示可选择按钮
            DisplayEventButtons();

            innerContentsButton.Interactable = false;

            // 初始化内容物
            if (currentDisplayedCard.TryGetComponent<InnerContentsComponent>(out var component))
            {
                innerContentsButton.gameObject.SetActive(true);
                innerBag.InitFromInnerContentComponent(component);
                innerContentsButton.Interactable = true;
            }
            else
            {
                innerContentsButton.gameObject.SetActive(false);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(menuLayout as RectTransform);

        // 显示详情
        DisplayDetails();

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
        detailsText.gameObject.SetActive(true);
        innerBag.gameObject.SetActive(false);
        // 显示卡牌详细信息
        detailsText.text = currentDisplayedCard.CardDesc;
        SelectWithTween(detailsButton.GetComponent<RectTransform>());
    }

    private void DisplayInnerContents()
    {
        detailsText.gameObject.SetActive(false);
        innerBag.gameObject.SetActive(true);
        SelectWithTween(innerContentsButton.GetComponent<RectTransform>());
    }

    private void DisplayEventButtons()
    {
        if (currentDisplayedCard == null) return;

        MonoUtility.DestroyAllChildren(buttonLayout);
        foreach (var e in currentDisplayedCard.Events)
        {
            GameObject buttonPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/CardEventButton");
            var button = Instantiate(buttonPrefab, buttonLayout).GetComponent<HoverableButton>();
            var btnText = button.GetComponentInChildren<Text>();
            btnText.text = e.name;

            bool interactable = e.Judge();
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
        }
    }

    private void Clear()
    {
        moved = false;
        slot.ClearSlot();

        // 关闭时如果卡牌有循环音将循环音减小
        if (currentDisplayedCard != null && currentDisplayedCard.HasLoopSound)
            currentDisplayedCard.OnDetailClose();

        if (currentDisplayedCard != null)
            currentDisplayedCard.TempSlotTransform = null;

        currentDisplayedCard = null;
        detailsText.text = "";
        innerBag.Clear();
        innerBag.gameObject.SetActive(false);
        innerContentsButton.gameObject.SetActive(false);
        MonoUtility.DestroyAllChildren(buttonLayout);
    }

    private void SelectWithTween(RectTransform target)
    {
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
