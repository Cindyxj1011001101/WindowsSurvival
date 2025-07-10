using UnityEngine;
using UnityEngine.UI;

public class DetailsWindow : WindowBase
{
    private Text detailsText;
    private Transform buttonLayout;
    private Transform tagLayout;
    private CardSlot slot;
    private CardSlot sourceSlot;
    private CardInstance currentDisplayedCard;

    protected override void Awake()
    {
        base.Awake();

        slot = transform.Find("Content/CardSlot").GetComponent<CardSlot>();
        detailsText = transform.Find("Content/Details").GetComponent<Text>();
        buttonLayout = transform.Find("Content/ButtonLayout");
        tagLayout = transform.Find("Content/TagLayout");
        // 禁止拖拽
        slot.GetComponentInChildren<CardDragHandler>().enabled = false;
        // 禁止双击
        slot.GetComponentInChildren<DoubleClickHandler>().enabled = false;
    }

    protected override void Init()
    {
    }

    public void Refresh(CardSlot sourceSlot)
    {
        // 清除原数据
        Clear();

        if (sourceSlot.StackCount <= 0) return;

        // 记录sourceSlot和当前显示的卡牌
        this.sourceSlot = sourceSlot;
        currentDisplayedCard = sourceSlot.PeekCard();

        // 显示卡牌
        slot.DisplayCard(currentDisplayedCard);

        // 显示卡牌标签
        CardData cardData = currentDisplayedCard.GetCardData();
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
            GameObject buttonPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/Button");
            Button button = Instantiate(buttonPrefab, buttonLayout).GetComponent<Button>();
            button.interactable = false;
            button.GetComponentInChildren<Text>().text = cardEvent.EventName;

            // 判断cardEvent是否满足条件
            if (EffectResolve.Instance.ConditionEventJudge(cardEvent))
            {
                button.onClick.AddListener(() =>
                {
                    currentDisplayedCard.Use();
                    EffectResolve.Instance.Resolve(cardEvent);
                    Refresh(sourceSlot);
                });
                button.interactable = true;
            }
        }
    }

    private void Clear()
    {
        slot.ClearSlot();
        sourceSlot = null;
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
