using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DetailsWindow : WindowBase
{
    [SerializeField] private Text detailsText;
    [SerializeField] private Transform buttonLayout;
    [SerializeField] private CardSlot slot;
    [SerializeField] private InnerBag innerBag;

    [SerializeField] private HoverableButton detailsButton; // 显示详细信息按钮
    [SerializeField] private HoverableButton innerContentsButton; // 显示内部内容按钮

    [SerializeField] private RectTransform selectRect; // 选择框

    private Card currentDisplayedCard;

    protected override void Awake()
    {
        base.Awake();
        EventManager.Instance.AddListener<EnvironmentBag>(EventType.Move, OnMove);
        EventManager.Instance.AddListener<ChangePlayerBagCardsArgs>(EventType.ChangePlayerBagCards, OnPlayerCardsChanged);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<EnvironmentBag>(EventType.Move, OnMove);
        EventManager.Instance.RemoveListener<ChangePlayerBagCardsArgs>(EventType.ChangePlayerBagCards, OnPlayerCardsChanged);
    }

    protected override void Init()
    {
        if (currentDisplayedCard == null)
        {
            Clear();
            innerBag.Clear();
        }

        detailsButton.onClick.AddListener(() =>
        {
            if (currentDisplayedCard != null)
            {
                DisplayDetails();
                SelectWithTween(detailsButton.GetComponent<RectTransform>());
            }
        });

        innerContentsButton.onClick.AddListener(() =>
        {
            if (currentDisplayedCard != null)
            {
                DisplayInnerContents();
                SelectWithTween(innerContentsButton.GetComponent<RectTransform>());
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
            Refresh(currentDisplayedCard.Slot);
    }

    bool moved = false;
    private void OnMove(EnvironmentBag curEnvironmentBag)
    {
        // 切地点时清除显示
        Clear();
        moved = true;
    }

    public void Refresh(CardSlot sourceSlot)
    {
        // 清除原数据
        Clear();

        if (sourceSlot == null || sourceSlot.StackNum <= 0) return;

        // 记录当前显示的卡牌
        currentDisplayedCard = sourceSlot.PeekCard();

        // 显示卡牌
        slot.DisplayCard(currentDisplayedCard, 1);

        EventManager.Instance.TriggerEvent(EventType.DialogueCondition, new SubscribeActionArgs("Detail", currentDisplayedCard.CardName));

        // 显示可选择按钮
        foreach (var e in currentDisplayedCard.Events)
        {
            GameObject buttonPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/CardEventButton");
            var button = Instantiate(buttonPrefab, buttonLayout).GetComponent<HoverableButton>();
            var btnText = button.GetComponentInChildren<Text>();
            btnText.text = e.name;

            // 判断cardEvent是否满足条件
            if (e.Judge())
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    var sourceSlot = currentDisplayedCard.Slot;
                    // 先执行事件
                    e.Inovke();
                    // 如果地点发生改变则不刷新
                    if (!moved)
                    {
                        // 再刷新
                        Refresh(sourceSlot);
                        moved = false;
                    }
                });
                button.Interactable = true;
            }
            else
            {
                button.Interactable = false;
                btnText.color = ColorManager.darkGrey;
            }
        }

        DisplayDetails();

        innerContentsButton.Interactable = false;

        // 初始化内容物
        if (currentDisplayedCard.TryGetComponent<InnerContentsComponent>(out var component))
        {
            innerBag.InitFromInnerContentComponent(component);
            innerContentsButton.Interactable = true;
        }
        else
        {
            innerContentsButton.ChangeColor(ColorManager.darkGrey);
        }
    }

    private void DisplayDetails()
    {
        detailsText.gameObject.SetActive(true);
        innerBag.gameObject.SetActive(false);
        // 显示卡牌详细信息
        detailsText.text = currentDisplayedCard.CardDesc;
    }

    private void DisplayInnerContents()
    {
        detailsText.gameObject.SetActive(false);
        innerBag.gameObject.SetActive(true);
    }

    private void Clear()
    {
        slot.ClearSlot();
        currentDisplayedCard = null;
        detailsText.text = "";
        innerBag.Clear();
        MonoUtility.DestroyAllChildren(buttonLayout);
    }

    private void SelectWithTween(RectTransform target)
    {
        Vector2 targetPos = new (target.anchoredPosition.x, selectRect.anchoredPosition.y);

        selectRect.DOKill();
        selectRect.DOAnchorPos(targetPos, 0.2f).SetEase(Ease.OutQuad);
    }
}
