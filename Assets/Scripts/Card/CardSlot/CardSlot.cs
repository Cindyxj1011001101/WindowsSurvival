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
    [SerializeField] private RectTransform innerContentsComponent; // 用于显示内容物组件
    [SerializeField] private UIStateSlider temperatureSlider; // 温度组件
    [SerializeField] private CanvasGroup cardCanvasGroup;
    [SerializeField] private Text moreInfoText; // 额外信息
    [SerializeField] private RectTransform particleDisplayRect; // 显示粒子的区域
    [SerializeField] private GameObject mask;

    private Dictionary<Type, float> lastComponentValues = new();
    private Dictionary<Type, UIStateSlider> componentSliders = new(); // 用于存储组件的滑动条

    public SlotCards Cards { get; protected set; }
    public bool IsEmpty => Cards.IsEmpty;
    public int StackNum => Cards.StackNum;

    public bool Interactable {  get; protected set; }

    private bool dontRefresh;

    public bool DontRefresh
    {
        get => dontRefresh;
        set
        {
            dontRefresh = value;
            if (!value) RefreshDisplay();
        }
    }

    private void OnEnable()
    {
        Clear();
        EventManager.Instance.AddListener(EventType.StartChangeTime, OnChangeTimeStarted);
        EventManager.Instance.AddListener(EventType.EndChangeTime, OnChangeTimeEnded);
        EventManager.Instance.AddListener<Card>(EventType.PickUpCard, OnCardPickedUp);
        EventManager.Instance.AddListener(EventType.PutDownCard, OnCardPutDown);
    }

    private void OnDisable()
    {
        Clear();
        Cards?.SetCardSlot(null);

        EventManager.Instance.RemoveListener(EventType.StartChangeTime, OnChangeTimeStarted);
        EventManager.Instance.RemoveListener(EventType.EndChangeTime, OnChangeTimeEnded);
        EventManager.Instance.AddListener<Card>(EventType.PickUpCard, OnCardPickedUp);
        EventManager.Instance.AddListener(EventType.PutDownCard, OnCardPutDown);
    }

    public void Init(SlotCards slotCards)
    {
        Cards = slotCards;
        slotCards.SetCardSlot(this);
        RefreshDisplay();
    }

    #region 显示

    private void OnCardPickedUp(Card card)
    {
        if (Cards == null) return;

        if (IsEmpty)
        {
            // 同背包或者卡牌可以放置到不同背包
            if (card.Bag == Cards.Bag || card.Moveable && Cards.Bag.CanAddCard(card, out _))
                mask.SetActive(false);
            else
                mask.SetActive(true);
            return;
        }

        if (PeekCard().CanQuickInteract(card))
        {
            mask.SetActive(false);
            Interactable = true;
        }
        else
        {
            mask.SetActive(true);
            Interactable = false;
        }
    }

    private void OnCardPutDown()
    {
        mask.SetActive(false);
        Interactable = false;
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
        foreach (var (type, lastValue) in lastComponentValues)
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

    /// <summary>
    /// 刷新当前显示
    /// </summary>
    public void RefreshDisplay()
    {
        if (dontRefresh) return;

        mask.SetActive(false);
        Interactable = false;

        if (IsEmpty)
        {
            Clear();
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
        if (component is TemperatureComponent t)
        {
            temperatureSlider.gameObject.SetActive(true);
            if (t.temperature <= 50)
            {
                temperatureSlider.fillColor = ColorManager.Green;
            }
            else if (t.temperature <= 100)
            {
                temperatureSlider.fillColor = ColorManager.Yellow;
            }
            else if (t.temperature <= 200)
            {
                temperatureSlider.fillColor = ColorManager.Orange;
            }
            else
            {
                temperatureSlider.fillColor = ColorManager.Red;
            }
            temperatureSlider.SetValue(t.temperature, t.maxTemperature);
            temperatureSlider.tipController.SetTip($"当前温度:    {t.temperature}/{t.maxTemperature}", temperatureSlider.fillColor);
            return;
        }

        if (!componentSliders.TryGetValue(component.GetType(), out UIStateSlider slider))
        {
            slider = ObjectBufferPool.Instance.Get("Prefabs/UI/Controls/Components", "Component", valueComponentLayout).GetComponent<UIStateSlider>();
            slider.transform.SetAsLastSibling();
            componentSliders.Add(component.GetType(), slider);
        }

        slider.fillColor = ColorManager.CardComponentColors[component.GetType()];

        switch (component)
        {
            case DurabilityComponent durabilityComponent:
                slider.SetValue(durabilityComponent.durability, durabilityComponent.maxDurability);
                slider.tipController.SetTip($"耐久度:    {slider.value * 100:0.0}%", slider.fillColor);
                break;
            case FreshnessComponent freshnessComponent:
                slider.SetValue(freshnessComponent.freshness, freshnessComponent.maxFreshness);
                slider.tipController.SetTip($"新鲜度:    {slider.value * 100:0.0}%", slider.fillColor);
                break;
            case GrowthComponent growthComponent:
                slider.SetValue(growthComponent.growth, growthComponent.maxGrowth);
                slider.tipController.SetTip($"生长度:    {slider.value * 100:0.0}%", slider.fillColor);
                break;
            case ProgressComponent progressComponent:
                slider.SetValue(progressComponent.progress, progressComponent.maxProgress);
                slider.tipController.SetTip($"产物进度:    {slider.value * 100:0.0}%", slider.fillColor);
                break;
            case FuelContainerComponent fuelContainerComponent:
                slider.SetValue(fuelContainerComponent.fuel, fuelContainerComponent.maxFuel);
                slider.tipController.SetTip($"剩余燃料:    {slider.value * 100:0.0}%", slider.fillColor);
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
        innerContentsComponent.gameObject.SetActive(true);
        for (int i = 0; i < innerContentsComponent.childCount; i++)
        {
            innerContentsComponent.GetChild(i).gameObject.SetActive(i < component.bag.SlotCount);
            innerContentsComponent.GetChild(i).GetComponent<Image>().color = i < component.bag.SlotCount - component.bag.EmptySlotCount ? ColorManager.White : ColorManager.DarkGrey;
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
            Clear();
            return;
        }

        EnableDisplay();

        DisplayCardImage(card.CardImage, card.IsBigIcon);
        nameText.text = card.CardName;

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
        // 显示燃料数量
        if (card.TryGetComponent<FuelContainerComponent>(out var fc))
            DisplayValueComponent(fc);
        // 显示温度
        if (card.TryGetComponent<TemperatureComponent>(out var t))
            DisplayValueComponent(t);

        // 显示额外信息
        moreInfoText.text = card.ExtraInfo;
    }

    /// <summary>
    /// 不显示卡牌
    /// </summary>
    public void Clear()
    {
        mask.SetActive(false);
        cardCanvasGroup.alpha = 0;
        cardCanvasGroup.blocksRaycasts = false;
        cardCanvasGroup.interactable = false;
        ObjectBufferPool.Instance.RestoreAllChildren(valueComponentLayout);
        if (innerContentsComponent != null)
            innerContentsComponent.gameObject.SetActive(false);
        if (temperatureSlider != null)
            temperatureSlider.gameObject.SetActive(false);
        componentSliders.Clear();
        lastComponentValues.Clear();
    }

    /// <summary>
    /// 允许显示卡牌
    /// </summary>
    private void EnableDisplay()
    {
        mask.SetActive(false);
        cardCanvasGroup.alpha = 1;
        cardCanvasGroup.blocksRaycasts = true;
        cardCanvasGroup.interactable = true;
    }

    public void ShowTip(string tip)
    {
        MFXUtility.ShowTip(tip, transform.position + (transform as RectTransform).sizeDelta.y * 0.35f * Vector3.up);
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

    private int CalcLevel(float value)
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
    public bool ContainsByCardName(string cardName) => Cards.ContainsByCardName(cardName);

    /// <summary>
    /// 判断该卡牌格是否放有相同卡牌（ID相同）
    /// </summary>
    /// <param name="cardId"></param>
    /// <returns></returns>
    public bool ContainsByCardId(string cardId) => Cards.ContainsByCardId(cardId);

    /// <summary>
    /// 能否添加指定卡牌，只有id相同才能堆叠
    /// </summary>
    /// <returns></returns>
    public virtual bool CanAddCard(Card card) => Cards.CanAddCard(card);

    /// <summary>
    /// 添加一张卡牌
    /// </summary>
    /// <param name="card"></param>
    public virtual void AddCard(Card card) => Cards.AddCard(card);

    /// <summary>
    /// 移除指定的一张卡牌
    /// </summary>
    /// <param name="card"></param>
    public virtual void RemoveCard(Card card) => Cards.RemoveCard(card);

    /// <summary>
    /// 移除最优先显示的卡牌
    /// </summary>
    /// <returns></returns>
    public Card RemoveCard() => Cards.RemoveCard();

    /// <summary>
    /// 移除指定数量的卡牌
    /// </summary>
    /// <param name="amount"></param>
    public void RemoveCards(int amount) => Cards.RemoveCards(amount);

    /// <summary>
    /// 取得优先级最高的卡牌
    /// </summary>
    /// <returns></returns>
    public Card PeekCard() => Cards.PeekCard();
}