using UnityEngine;
using UnityEngine.UI;

public class DetailsWindow : WindowBase
{
    [SerializeField] private Text detailsText;
    [SerializeField] private Transform buttonLayout;
    [SerializeField] private Transform tagLayout;
    [SerializeField] private CardSlot slot;
    //private CardSlot sourceSlot;
    private CardInstance currentDisplayedCard;

    protected override void Awake()
    {
        base.Awake();

        //slot = transform.Find("Content/CardSlot").GetComponent<CardSlot>();
        //detailsText = transform.Find("Content/Details").GetComponent<Text>();
        //buttonLayout = transform.Find("Content/ButtonLayout");
        //tagLayout = transform.Find("Content/TagLayout");
        //// 禁止拖拽
        //slot.GetComponentInChildren<CardDragHandler>().enabled = false;
        //// 禁止双击
        //slot.GetComponentInChildren<DoubleClickHandler>().enabled = false;

        EventManager.Instance.AddListener<EnvironmentBag>(EventType.Move, OnMove);
        EventManager.Instance.AddListener<ChangePlayerBagCardsArgs>(EventType.ChangePlayerBagCards, OnPlayerBagCardsChanged);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<EnvironmentBag>(EventType.Move, OnMove);
        EventManager.Instance.RemoveListener<ChangePlayerBagCardsArgs>(EventType.ChangePlayerBagCards, OnPlayerBagCardsChanged);
    }

    protected override void Init()
    {
    }

    private void OnMove(EnvironmentBag curEnvironmentBag)
    {
        // 切地点时，如果窗口中不是正在显示玩家背包中的物品，则清除显示
        //if (sourceSlot.Bag is not PlayerBag)
            Clear();
    }

    private void OnPlayerBagCardsChanged(ChangePlayerBagCardsArgs args)
    {
        if (currentDisplayedCard != null)
            Refresh(currentDisplayedCard.Slot);
    }

    public void Refresh(CardSlot sourceSlot)
    {
        // 清除原数据
        Clear();

        if (sourceSlot == null || sourceSlot.StackCount <= 0) return;

        // 记录sourceSlot和当前显示的卡牌
        //this.sourceSlot = sourceSlot;
        currentDisplayedCard = sourceSlot.PeekCard();

        // 显示卡牌
        slot.DisplayCard(currentDisplayedCard);

        // 显示卡牌标签
        CardData cardData = currentDisplayedCard.CardData;
        foreach (var tag in cardData.CardTagList)
        {
            GameObject tagPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/Tags/" + tag.ToString());
            Instantiate(tagPrefab, tagLayout);
        }

        // 显示卡牌详细信息
        detailsText.text = cardData.cardDesc;

        // 显示可选择按钮
        foreach (var cardEvent in cardData.cardEventList)
        {
            GameObject buttonPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/CardEventButton");
            Button button = Instantiate(buttonPrefab, buttonLayout).GetComponent<Button>();
            button.interactable = false;
            button.GetComponentInChildren<Text>().text = cardEvent.EventName;

            // 判断cardEvent是否满足条件
            if (GameManager.Instance.CanCardEventInvoke(cardEvent))
            {
                button.onClick.AddListener(() =>
                {
                    var sourceSlot = currentDisplayedCard.Slot;

                    // 先使用
                    currentDisplayedCard.Use();
                    // 如果该卡牌事件是需要其他卡牌配合触发的，如使用工具
                    if (cardEvent is ConditionalCardEvent)
                    {
                        // 遍历需要使用到的工具
                        foreach (var condition in (cardEvent as ConditionalCardEvent).ConditionCardList)
                        {
                            // 尝试从玩家背包中取得所需工具
                            var slot = GameManager.Instance.PlayerBag.TryGetCardByCondition(condition);
                            slot.PeekCard().Use();
                            // 只需要使用一次工具，所以这里break
                            break;
                        }
                    }
                    // 再刷新
                    Refresh(sourceSlot);
                    // 最后触发效果
                    GameManager.Instance.HandleCardEvent(cardEvent);
                });
                button.interactable = true;
            }
        }
    }

    private void Clear()
    {
        slot.ClearSlot();
        //sourceSlot = null;
        currentDisplayedCard = null;
        detailsText.text = "";
        for (int i = 0; i < buttonLayout.childCount; i++)
        {
            Destroy(buttonLayout.GetChild(i).gameObject);
        }
        for (int i = 0; i < tagLayout.childCount; i++)
        {
            Destroy(tagLayout.GetChild(i).gameObject);
        }
    }
}
