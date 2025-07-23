using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 卡牌格
/// </summary>
public class CardSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text nameText;
    [SerializeField] private GameObject stackObject; // 控制是否显示堆叠
    [SerializeField] private Text stackNumText; // 显示数量
    [SerializeField] private Image maxStackNumImage; // 显示最大堆叠数量的图标
    [SerializeField] private VerticalLayoutGroup componentLayout; // 用于显示新鲜度、耐久等组件的布局
    [SerializeField] private CanvasGroup cardCanvasGroup;
    [SerializeField] private Text moreInfoText; // 额外信息

    private Dictionary<ICardComponent, Slider> componentSliders = new(); // 用于存储组件的滑动条

    private List<Card> cards = new();
    public bool IsEmpty => cards.Count == 0;
    public int StackNum => cards.Count;

    public List<Card> Cards => cards;

    private BagBase bag;
    public BagBase Bag => bag;

    private void Awake()
    {
        if (cardCanvasGroup.TryGetComponent<DoubleClickHandler>(out var doubleClickHandler))
        {
            doubleClickHandler.onDoubleClick.AddListener(() =>
            {
                (WindowsManager.Instance.OpenWindow("Details") as DetailsWindow).Refresh(this);
            });
        }

        EventManager.Instance.AddListener(EventType.ChangeCardProperty, RefreshCurrentDisplay);
    }

    public void SetBag(BagBase bag)
    {
        this.bag = bag;
    }

    public void InitFromRuntimeData(CardSlotRuntimeData cardSlotRuntimeData)
    {
        foreach (var card in cardSlotRuntimeData.cardList)
        {
            AddCard(card);
            card.StartUpdating();
        }
    }

    #region 显示

    /// <summary>
    /// 刷新当前显示
    /// </summary>
    public void RefreshCurrentDisplay()
    {
        if (IsEmpty) return;

        DisplayCard(PeekCard(), StackNum);
    }

    private void DisplayCardImage(Sprite sprite, bool isBigIcon)
    {
        iconImage.sprite = sprite;
        // 设置原始大小
        iconImage.SetNativeSize();
        Vector2 offset = isBigIcon ? new Vector2(16, -16) : new Vector2(30, -30);
        (iconImage.transform as RectTransform).anchoredPosition = offset;
    }

    private void DisplayStackNum(int stackNum, int maxStackNum, bool displayStack)
    {

        if (maxStackNum <= 1 || !displayStack)
        {
            stackObject.SetActive(false);
            maxStackNumImage.gameObject.SetActive(false);
        }
        else
        {
            stackObject.SetActive(true);
            stackNumText.text = $"{stackNum}";

            maxStackNumImage.gameObject.SetActive(stackNum == maxStackNum);
        }
    }

    private void DisplayComponent(ICardComponent component)
    {
        if (!componentSliders.TryGetValue(component, out Slider slider))
        {
            var prefab = Resources.Load<GameObject>("Prefabs/UI/Controls/Components/" + component.GetType().Name);
            slider = Instantiate(prefab, componentLayout.transform).GetComponent<Slider>();
            componentSliders.Add(component, slider);
        }

        switch (component)
        {
            case DurabilityComponent durabilityComponent:
                slider.value = (float)durabilityComponent.durability / durabilityComponent.maxDurability;
                break;
            case FreshnessComponent freshnessComponent:
                slider.value = (float)freshnessComponent.freshness / freshnessComponent.maxFreshness;
                break;
            case GrowthComponent growthComponent:
                slider.value = (float)growthComponent.growth / growthComponent.maxGrowth;
                break;
            case ProgressComponent progressComponent:
                slider.value = (float)progressComponent.progress / progressComponent.maxProgress;
                break;
            default:
                Debug.LogWarning($"未知组件类型: {component.GetType()}");
                break;
        }
    }

    /// <summary>
    /// 显示指定数量的卡牌
    /// </summary>
    /// <param name="card"></param>
    /// <param name="stackCount"></param>
    public void DisplayCard(Card card, int stackCount, bool displayStack = true)
    {
        // 如果要显示的数量小于等于零，则什么也不显示
        if (stackCount <= 0)
        {
            DisableDisplay();
            return;
        }

        EnableDisplay();

        DisplayCardImage(card.CardImage, card.IsBigIcon);
        nameText.text = card.CardName;

        // 显示堆叠数量
        DisplayStackNum(stackCount, card.MaxStackNum, displayStack);

        moreInfoText.text = "";

        // 显示耐久
        if (card.TryGetComponent<DurabilityComponent>(out var d))
            DisplayComponent(d);
        // 显示新鲜度
        if (card.TryGetComponent<FreshnessComponent>(out var f))
            DisplayComponent(f);
        // 显示生长度
        if (card.TryGetComponent<GrowthComponent>(out var g))
            DisplayComponent(g);
        // 显示产物进度
        if (card.TryGetComponent<ProgressComponent>(out var p))
            DisplayComponent(p);
    }

    /// <summary>
    /// 不显示卡牌
    /// </summary>
    private void DisableDisplay()
    {
        cardCanvasGroup.alpha = 0;
        cardCanvasGroup.blocksRaycasts = false;
        cardCanvasGroup.interactable = false;
    }

    /// <summary>
    /// 允许显示卡牌
    /// </summary>
    private void EnableDisplay()
    {
        cardCanvasGroup.alpha = 1;
        cardCanvasGroup.blocksRaycasts = true;
        cardCanvasGroup.interactable = true;
    }
    #endregion

    /// <summary>
    /// 判断该卡牌格是否放有同类卡牌（名称相同即同类）
    /// </summary>
    /// <param name="cardName"></param>
    /// <returns></returns>
    public bool ContainsByCardName(string cardName) => !IsEmpty && cardName == cards[0].CardName;

    /// <summary>
    /// 判断该卡牌格是否放有相同卡牌（ID相同）
    /// </summary>
    /// <param name="cardId"></param>
    /// <returns></returns>
    public bool ContainsByCardId(string cardId) => !IsEmpty && cardId == cards[0].CardId;

    /// <summary>
    /// 能否添加指定卡牌，只有id相同才能堆叠
    /// </summary>
    /// <returns></returns>
    public virtual bool CanAddCard(Card card)
    {
        return IsEmpty || (card.CardId == cards[0].CardId && StackNum < card.MaxStackNum);
    }

    /// <summary>
    /// 得到对于某张卡牌的剩余容量
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    public int GetRemainingCapacity(Card card)
    {
        // 如果当前slot为空，剩余容量为card的最大堆叠数量
        if (IsEmpty) return card.MaxStackNum;
        // 如果当前slot不为空，并且不可以堆叠该卡牌，则剩余容量为0
        if (!CanAddCard(card)) return 0;
        // 剩余容量为最大堆叠数 - 当前堆叠数
        return card.MaxStackNum - StackNum;
    }

    /// <summary>
    /// 添加一张卡牌
    /// </summary>
    /// <param name="card"></param>
    public virtual void AddCard(Card card)
    {
        cards.Add(card);
        cards.Sort((a, b) => a.CompareTo(b));

        RefreshCurrentDisplay();

        // 当卡牌添加到玩家背包时
        if (bag is PlayerBag)
            EventManager.Instance.TriggerEvent(EventType.ChangePlayerBagCards,
                new ChangePlayerBagCardsArgs { card = card, add = 1 });

        card.SetCardSlot(this);
    }

    /// <summary>
    /// 移除指定的一张卡牌
    /// </summary>
    /// <param name="card"></param>
    public virtual void RemoveCard(Card card)
    {
        if (!cards.Contains(card)) return;

        cards.Remove(card);
        card.SetCardSlot(null);

        if (StackNum == 0)
            ClearSlot();
        else
            RefreshCurrentDisplay();

        // 当卡牌从玩家背包移除时
        if (bag is PlayerBag)
            EventManager.Instance.TriggerEvent(EventType.ChangePlayerBagCards,
                new ChangePlayerBagCardsArgs { card = card, add = -1 });
    }

    /// <summary>
    /// 移除最优先显示的卡牌
    /// </summary>
    /// <returns></returns>
    public Card RemoveCard()
    {
        var cardToRemove = cards[0];

        RemoveCard(cardToRemove);

        return cardToRemove;
    }

    /// <summary>
    /// 移除指定数量的卡牌
    /// </summary>
    /// <param name="amount"></param>
    public void RemoveCards(int amount)
    {
        for (int i = 0; i < amount; i++)
            RemoveCard();
    }

    public void TransferCardsTo(CardSlot other, int count)
    {
        for (int i = 0; i < count; i++)
        {
            other.AddCard(RemoveCard());
        }
    }

    public Card PeekCard() => cards[0];

    public void ClearSlot()
    {
        cards.Clear();
        componentSliders.Clear();
        MonoUtility.DestroyAllChildren(componentLayout.transform);
        DisableDisplay();
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EventType.ChangeCardProperty, RefreshCurrentDisplay);
    }
}