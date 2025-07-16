using UnityEngine;
using UnityEngine.UI;

public class DetailsWindow : WindowBase
{
    [SerializeField] private Text detailsText;
    [SerializeField] private Transform buttonLayout;
    [SerializeField] private Transform tagLayout;
    [SerializeField] private CardSlot slot;
    private Card currentDisplayedCard;

    protected override void Awake()
    {
        base.Awake();
        EventManager.Instance.AddListener<EnvironmentBag>(EventType.Move, OnMove);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<EnvironmentBag>(EventType.Move, OnMove);
    }

    protected override void Init()
    {
    }

    private void OnMove(EnvironmentBag curEnvironmentBag)
    {
        // 切地点时清除显示
        Clear();
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
        foreach (var tag in currentDisplayedCard.tags)
        {
            GameObject tagPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/Tags/" + tag.ToString());
            Instantiate(tagPrefab, tagLayout);
        }

        // 显示卡牌详细信息
        detailsText.text = currentDisplayedCard.cardDesc;

        // 显示可选择按钮
        foreach (var e in currentDisplayedCard.events)
        {
            GameObject buttonPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/CardEventButton");
            Button button = Instantiate(buttonPrefab, buttonLayout).GetComponent<Button>();
            button.interactable = false;
            button.GetComponentInChildren<Text>().text = e.name;

            // 判断cardEvent是否满足条件
            if (e.Judge())
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() =>
                {
                    var sourceSlot = currentDisplayedCard.slot;
                    // 先执行事件
                    e.Inovke();
                    // 再刷新
                    Refresh(sourceSlot);
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
