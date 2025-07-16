using UnityEngine;
using UnityEngine.UI;

public class DetailsWindow : WindowBase
{
    [SerializeField] private Text detailsText;
    [SerializeField] private Transform buttonLayout;
    [SerializeField] private Transform tagLayout;
    [SerializeField] private CardSlot slot;
    private CardInstance currentDisplayedCard;

    protected override void Awake()
    {
        base.Awake();
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
        // 切地点时清除显示
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

        // 记录当前显示的卡牌
        currentDisplayedCard = sourceSlot.PeekCard();

        // 显示卡牌
        slot.DisplayCard(currentDisplayedCard, 1);

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
        currentDisplayedCard = null;
        detailsText.text = "";
        MonoUtility.DestroyAllChildren(buttonLayout);
        MonoUtility.DestroyAllChildren(tagLayout);
    }
}
