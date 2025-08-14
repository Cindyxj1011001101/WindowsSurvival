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
    [SerializeField] private RectTransform valueComponentLayout; // 用于显示新鲜度、耐久等组件的布局
    [SerializeField] private RectTransform innerContentsComponentLayout; // 用于显示内容物组件
    [SerializeField] private CanvasGroup cardCanvasGroup;
    [SerializeField] private Text moreInfoText; // 额外信息
    [SerializeField] private RectTransform particleDisplayRect; // 显示粒子的区域
    public bool dontRefresh; // 是否不刷新显示（用于某些特殊情况）


    private Dictionary<Type, float> lastComponentValues = new();
    private Dictionary<Type, Slider> componentSliders = new(); // 用于存储组件的滑动条

    private List<Card> cards = new();
    public bool IsEmpty => cards.IsNullOrEmpty();
    public int StackNum => cards.Count;

    public List<Card> Cards => cards;

    private BagBase bag;
    public BagBase Bag => bag;

    private void Awake()
    {
        if (!dontRefresh)
            EventManager.Instance.AddListener(EventType.ChangeCardProperty, RefreshCurrentDisplay);
        EventManager.Instance.AddListener(EventType.StartChangeTime, OnChangeTimeStarted);
        EventManager.Instance.AddListener(EventType.EndChangeTime, OnChangeTimeEnded);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EventType.ChangeCardProperty, RefreshCurrentDisplay);
        EventManager.Instance.RemoveListener(EventType.StartChangeTime, OnChangeTimeStarted);
        EventManager.Instance.RemoveListener(EventType.EndChangeTime, OnChangeTimeEnded);
    }

    public void Init(List<Card> cardList)
    {
        cards = cardList;
        foreach (var card in cards)
        {
            card.SetCardSlot(this);
            card.StartUpdating();
        }
        RefreshCurrentDisplay();
    }

    public void SetBag(BagBase bag)
    {
        this.bag = bag;
    }

    private void OnChangeTimeStarted()
    {
        // 记录组件的初始值
        lastComponentValues.Clear();
        foreach (var (type, slider) in componentSliders)
        {
            lastComponentValues.Add(type, slider.value);
        }
    }

    private void OnChangeTimeEnded()
    {
        // 计算组件的变化值
        Dictionary<Type, float> deltaValues = new();
        foreach (var(type, lastValue) in lastComponentValues)
        {
            if (componentSliders.TryGetValue(type, out var slider))
            {
                if (slider.value != lastValue)
                    deltaValues.Add(type, slider.value - lastValue);
            }
        }

        if (deltaValues.Count > 0)
            DisplayComponentValuesChange(deltaValues);
        
        lastComponentValues.Clear();
    }

    #region 显示

    /// <summary>
    /// 刷新当前显示
    /// </summary>
    public void RefreshCurrentDisplay()
    {
        if (IsEmpty)
        {
            DisableDisplay();
            return;
        }

        DisplayCard(PeekCard(), StackNum);
    }

    private void DisplayCardImage(Sprite sprite, bool isBigIcon)
    {
        iconImage.sprite = sprite;
        Vector2 offset = isBigIcon ? new Vector2(16, -16) : new Vector2(30, -30);
        (iconImage.transform as RectTransform).anchoredPosition = offset;
        // 设置原始大小
        iconImage.SetNativeSize();
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

    /// <summary>
    /// 显示具有浮点值数据的组件
    /// </summary>
    /// <param name="component"></param>
    private void DisplayValueComponent(CardComponent component)
    {
        if (!componentSliders.TryGetValue(component.GetType(), out Slider slider))
        {
            var prefab = Resources.Load<GameObject>("Prefabs/UI/Controls/Components/" + component.GetType().Name);
            slider = Instantiate(prefab, valueComponentLayout).GetComponent<Slider>();
            componentSliders.Add(component.GetType(), slider);
        }

        
        if (!slider.TryGetComponent<HoverTipController>(out var tipController))
            tipController = slider.gameObject.AddComponent<HoverTipController>();

        switch (component)
        {
            case DurabilityComponent durabilityComponent:
                slider.value = (float)durabilityComponent.durability / durabilityComponent.maxDurability;
                tipController.SetTip($"耐久度:    {slider.value * 100:0.0}%", ColorManager.CardComponentColors[component.GetType()]);
                break;
            case FreshnessComponent freshnessComponent:
                slider.value = (float)freshnessComponent.freshness / freshnessComponent.maxFreshness;
                tipController.SetTip($"新鲜度:    {slider.value * 100:0.0}%", ColorManager.CardComponentColors[component.GetType()]);
                break;
            case GrowthComponent growthComponent:
                slider.value = (float)growthComponent.growth / growthComponent.maxGrowth;
                tipController.SetTip($"生长度:    {slider.value * 100:0.0}%", ColorManager.CardComponentColors[component.GetType()]);
                break;
            case ProgressComponent progressComponent:
                slider.value = (float)progressComponent.progress / progressComponent.maxProgress;
                tipController.SetTip($"产物进度:    {slider.value * 100:0.0}%", ColorManager.CardComponentColors[component.GetType()]);
                break;
            default:
                Debug.LogWarning($"未知组件类型: {component.GetType()}");
                break;
        }
    }

    /// <summary>
    /// 显示内容物组件
    /// </summary>
    /// <param name="component"></param>
    private void DisplayInnerContentsComponent(InnerContentsComponent component)
    {
        innerContentsComponentLayout.gameObject.SetActive(true);
        for (int i = 0; i < innerContentsComponentLayout.childCount; i++)
        {
            innerContentsComponentLayout.GetChild(i).gameObject.SetActive(i < component.innerContents.Count);
            innerContentsComponentLayout.GetChild(i).GetComponent<Image>().color = i < component.UsedSlotCount ? ColorManager.White : ColorManager.DarkGrey;
        }
    }

    /// <summary>
    /// 显示指定数量的卡牌
    /// </summary>
    /// <param name="card"></param>
    /// <param name="stackNum"></param>
    public void DisplayCard(Card card, int stackNum, bool displayStack = true)
    {
        // 如果要显示的数量小于等于零，则什么也不显示
        if (stackNum <= 0)
        {
            DisableDisplay();
            return;
        }

        EnableDisplay();

        DisplayCardImage(card.CardImage, card.IsBigIcon);
        nameText.text = card.CardName;

        // 销毁旧的组件显示
        //MonoUtility.DestroyAllChildren(componentLayout.transform);
        //componentSliders.Clear();

        // 显示堆叠数量
        DisplayStackNum(stackNum, card.MaxStackNum, displayStack);

        // 显示耐久
        if (card.TryGetComponent<DurabilityComponent>(out var d))
            DisplayValueComponent(d);
        // 显示新鲜度
        if (card.TryGetComponent<FreshnessComponent>(out var f))
            DisplayValueComponent(f);
        // 显示生长度
        if (card.TryGetComponent<GrowthComponent>(out var g))
            DisplayValueComponent(g);
        // 显示产物进度
        if (card.TryGetComponent<ProgressComponent>(out var p))
            DisplayValueComponent(p);
        // 显示内容物数量
        if (card.TryGetComponent<InnerContentsComponent>(out var i))
            DisplayInnerContentsComponent(i);

        // 显示额外信息
        moreInfoText.text = card.ExtraInfo;
    }

    /// <summary>
    /// 不显示卡牌
    /// </summary>
    private void DisableDisplay()
    {
        cardCanvasGroup.alpha = 0;
        cardCanvasGroup.blocksRaycasts = false;
        cardCanvasGroup.interactable = false;
        MonoUtility.DestroyAllChildren(valueComponentLayout);
        if (innerContentsComponentLayout != null)
            innerContentsComponentLayout.gameObject.SetActive(false);
        componentSliders.Clear();
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

    /// <summary>
    /// 显示卡牌如耐久度变化、
    /// </summary>
    /// <param name="minute"></param>
    public void DisplayComponentValuesChange(Dictionary<Type, float> deltaValues)
    {
        List<(bool up, int level, Color color)> groups = new();

        foreach (var (type, deltaValue) in deltaValues)
        {
            groups.Add((deltaValue > 0, CalcLevel(deltaValue), ColorManager.CardComponentColors[type]));
        }

        MFXUtility.ShowArrows(particleDisplayRect, groups);
    }

    public void DisplayComponentValueChange(Type componentType, float value)
    {
        MFXUtility.ShowArrows(particleDisplayRect, value > 0, CalcLevel(value), ColorManager.CardComponentColors[componentType]);
    }

    private  int CalcLevel(float value)
    {
        var absValue = Mathf.Abs(value);
        if (absValue <= 0.1)
            return 1;
        else if (absValue <= 0.3)
            return 2;
        else
            return 3;
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
    /// 添加一张卡牌
    /// </summary>
    /// <param name="card"></param>
    public virtual void AddCard(Card card)
    {
        cards.Add(card);
        cards.Sort((a, b) => a.CompareTo(b));

        card.SetCardSlot(this);

        // 当卡牌添加到玩家背包时
        if (bag is PlayerBag || bag is EquipmentBag)
        {
            StateManager.Instance.ChangePlayerState(PlayerStateEnum.Load, card.Weight);

            EventManager.Instance.TriggerEvent(EventType.ChangePlayerBagCards,
                new ChangePlayerBagCardsArgs { card = card, add = 1 });
        }
    }

    /// <summary>
    /// 移除指定的一张卡牌
    /// </summary>
    /// <param name="card"></param>
    public virtual void RemoveCard(Card card)
    {
        if (!cards.Contains(card)) return;

        cards.Remove(card);
        //card.SetCardSlot(null);

        // 当卡牌从玩家背包移除时
        if (bag is PlayerBag || bag is EquipmentBag)
        {
            StateManager.Instance.ChangePlayerState(PlayerStateEnum.Load, -card.Weight);

            EventManager.Instance.TriggerEvent(EventType.ChangePlayerBagCards,
                new ChangePlayerBagCardsArgs { card = card, add = -1 });
        }
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

    public Card PeekCard() => cards[0];

    public void ClearSlot()
    {
        foreach (var card in cards)
        {
            card.SetCardSlot(null);
        }
        cards = new List<Card>(); // 避免影响其他引用
        lastComponentValues.Clear();
        DisableDisplay();
    }

    public void ShowTip(string tip, Color color)
    {
        MFXUtility.ShowTip(tip, transform.position + (transform as RectTransform).sizeDelta.y * 0.55f * Vector3.up, color);
    }

    public void ShowTip(string tip)
    {
        MFXUtility.ShowTip(tip, transform.position + (transform as RectTransform).sizeDelta.y * 0.55f * Vector3.up);
    }
}