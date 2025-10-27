using DG.Tweening;
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
    [SerializeField] private CanvasGroup cardCanvasGroup;
    [SerializeField] private Text moreInfoText; // 额外信息
    [SerializeField] private RectTransform particleDisplayRect; // 显示粒子的区域
    [SerializeField] private GameObject mask;

    [SerializeField] private RectTransform middle; // 用于显示新鲜度、耐久等组件的布局
    [SerializeField] private RectTransform left; // 用于显示计时器和盐水
    [SerializeField] private RectTransform right; // 用于显示温度和淡水
    [SerializeField] private RectTransform innerContentsComponent; // 用于显示内容物组件
    [SerializeField] private GameObject iconLayout; // 用于显示图标的布局
    [SerializeField] private Image fireIcon; // 图标上的火焰
    [SerializeField] private Image flashIcon; // 图标上的闪电

    [SerializeField] private Animator cardAnimator;

    [SerializeField] private HoverTipController tipController;

    [SerializeField] private bool onlyDisplay = false;

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
        if (onlyDisplay) return;

        Clear();
        EventManager.Instance.AddListener(EventType.StartChangeTime, OnChangeTimeStarted);
        EventManager.Instance.AddListener(EventType.EndChangeTime, OnChangeTimeEnded);
        EventManager.Instance.AddListener<Card>(EventType.PickUpCard, OnCardPickedUp);
        EventManager.Instance.AddListener(EventType.PutDownCard, OnCardPutDown);
    }

    private void OnDisable()
    {
        if (onlyDisplay) return;

        Clear();
        Cards?.SetCardSlot(null);

        transform.DOKill();
        transform.localScale = Vector3.one;

        EventManager.Instance.RemoveListener(EventType.StartChangeTime, OnChangeTimeStarted);
        EventManager.Instance.RemoveListener(EventType.EndChangeTime, OnChangeTimeEnded);
        EventManager.Instance.RemoveListener<Card>(EventType.PickUpCard, OnCardPickedUp);
        EventManager.Instance.RemoveListener(EventType.PutDownCard, OnCardPutDown);
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

        if (PeekCard().CanQuickInteract(card, out var tip))
        {
            mask.SetActive(false);
            Interactable = true;
            tipController.enabled = true;
            tipController.SetTip(tip);
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
        tipController.enabled = false;
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

        // 设置组件布局的中心点
        var posY = isBigIcon ? 57 : 65;
        middle.anchoredPosition = new Vector2(middle.anchoredPosition.x, posY);
    }

    private void DisplayStackNum(int stackNum, int maxStackNum, bool displayStack)
    {
        static int CountDigitFour(int number)
        {
            string numberStr = Math.Abs(number).ToString(); // 处理负数
            int count = 0;

            foreach (char c in numberStr)
            {
                if (c == '4')
                {
                    count++;
                }
            }

            return count;
        }

        if (maxStackNum <= 1 || !displayStack)
        {
            stackObject.SetActive(false);
            maxStackNumImage.gameObject.SetActive(false);
        }
        else
        {
            stackObject.SetActive(true);
            stackNumText.text = $"{stackNum}";
            stackNumText.GetComponent<TextSpacing>().spacing_x = -CountDigitFour(stackNum) * 2; // 处理字体中数字4导致的间距变大

            maxStackNumImage.gameObject.SetActive(stackNum == maxStackNum);
        }
    }

    /// <summary>
    /// 显示具有浮点值数据的组件
    /// </summary>
    /// <param name="component"></param>
    private void DisplayContinuousValueComponent(CardComponent component, RectTransform parent, bool vertical = false)
    {
        if (!componentSliders.TryGetValue(component.GetType(), out UIStateSlider slider))
        {
            if (component is TemperatureComponent)
                slider = ObjectBufferPool.Instance.Get("Prefabs/UI/Controls/Components", nameof(TemperatureComponent), parent).GetComponent<UIStateSlider>();
            else if (component is TimerComponent)
                slider = ObjectBufferPool.Instance.Get("Prefabs/UI/Controls/Components", nameof(TimerComponent), parent).GetComponent<UIStateSlider>();
            else if (component is CoordinateComponent)
                slider = ObjectBufferPool.Instance.Get("Prefabs/UI/Controls/Components", nameof(CoordinateComponent), parent).GetComponent<UIStateSlider>();
            else
                slider = ObjectBufferPool.Instance.Get("Prefabs/UI/Controls/Components", $"{(vertical ? "Vertical" : "")}Component", parent).GetComponent<UIStateSlider>();

            slider.transform.SetAsLastSibling();

            slider.transform.position = parent.position;
            (slider.transform as RectTransform).anchoredPosition = Vector3.zero;
            slider.transform.localRotation = Quaternion.identity;
            componentSliders.Add(component.GetType(), slider);
        }

        if (ColorManager.CardComponentColors.TryGetValue(component.GetType(), out var fillColor))
            slider.fillColor = fillColor;

        switch (component)
        {
            case DurabilityComponent durabilityComponent:
                slider.SetValue(durabilityComponent.value, durabilityComponent.maxValue);
                slider.tipController.SetTip($"耐久度:  {slider.value * 100:0.0}%", slider.fillColor);
                break;
            case FreshnessComponent freshnessComponent:
                slider.SetValue(freshnessComponent.value, freshnessComponent.maxValue);
                slider.tipController.SetTip($"新鲜度:  {slider.value * 100:0.0}%", slider.fillColor);
                break;
            case GrowthComponent growthComponent:
                slider.SetValue(growthComponent.value, growthComponent.maxValue);
                slider.tipController.SetTip($"生长度:  {slider.value * 100:0.0}%", slider.fillColor);
                break;
            case PlantGrowthComponent plantGrowthComponent:
                slider.SetValue(plantGrowthComponent.value, plantGrowthComponent.maxValue);
                slider.tipController.SetTip($"生长度:  {slider.value * 100:0.0}%", slider.fillColor);
                break;
            case ProgressComponent progressComponent:
                slider.SetValue(progressComponent.value, progressComponent.maxValue);
                slider.tipController.SetTip($"产物进度:  {slider.value * 100:0.0}%", slider.fillColor);
                break;
            case FuelStorageComponent fuelStorageComponent:
                slider.SetValue(fuelStorageComponent.value, fuelStorageComponent.maxValue);
                string tip = $"剩余燃料:  {fuelStorageComponent.value}/{fuelStorageComponent.maxValue}";

                iconLayout.SetActive(true);
                fireIcon.gameObject.SetActive(true);
                fireIcon.color = fuelStorageComponent.isBurning ? ColorManager.BurntOrange : ColorManager.DarkGrey;

                // 显示燃料消耗
                tip += $"\n自然消耗:  -{fuelStorageComponent.basicFuelConsumption}/15min";
                if (StateManager.Instance.WaterLevel.CurValue > 0)
                    tip += $"\n地面积水:  -{fuelStorageComponent.extraFuelConsumptionWhenWaterLevelHigh}/15min";

                // TODO: 冰层季额外消耗
                slider.tipController.SetTip(tip, slider.fillColor);
                break;
            case TemperatureComponent temperatureComponent:
                if (temperatureComponent.value <= 50)
                {
                    slider.fillColor = ColorManager.Green;
                }
                else if (temperatureComponent.value <= 100)
                {
                    slider.fillColor = ColorManager.Yellow;
                }
                else if (temperatureComponent.value <= 200)
                {
                    slider.fillColor = ColorManager.Orange;
                }
                else
                {
                    slider.fillColor = ColorManager.Red;
                }
                slider.SetValue(temperatureComponent.value, temperatureComponent.maxValue);
                slider.tipController.SetTip($"当前温度:  {temperatureComponent.value}/{temperatureComponent.maxValue}", slider.fillColor);
                break;
            case OxygenStorageComponent oxygenStorageComponent:
                slider.SetValue(oxygenStorageComponent.value, oxygenStorageComponent.maxValue);
                slider.tipController.SetTip($"剩余氧气:  {oxygenStorageComponent.value}/{oxygenStorageComponent.maxValue}", slider.fillColor);
                break;
            case FreshWaterStorageComponent freshWaterStorageComponent:
                slider.SetValue(freshWaterStorageComponent.value, freshWaterStorageComponent.maxValue);
                slider.tipController.SetTip($"淡水储量:  {freshWaterStorageComponent.value}/{freshWaterStorageComponent.maxValue}", slider.fillColor);
                break;
            case SalineWaterStorageComponent salineWaterStorageComponent:
                slider.SetValue(salineWaterStorageComponent.value, salineWaterStorageComponent.maxValue);
                slider.tipController.SetTip($"盐水储量:  {salineWaterStorageComponent.value}/{salineWaterStorageComponent.maxValue}", slider.fillColor);
                break;
            case TimerComponent timerComponent:
                slider.SetValue(timerComponent.value, timerComponent.maxValue);
                var hour = Mathf.FloorToInt(timerComponent.value / 60);
                var minute = timerComponent.value % 60;
                string leftTime = "";
                if (hour == 0 && minute == 0)
                    leftTime = "0min";
                if (hour > 0)
                    leftTime += $"{hour}h";
                if (minute > 0)
                    leftTime += $"{minute}min";

                slider.tipController.SetTip($"距 {timerComponent.tipText} 剩余:  {leftTime}", slider.fillColor);
                break;
            case CoordinateComponent coordinateComponent:
                slider.SetValue(coordinateComponent.coordinate.Position, GameManager.Instance.CurEnvironmentBag.PlaceData.maxCoord);
                slider.tipController.SetTip($"当前坐标:  {coordinateComponent.coordinate.Position:0.0}\n距离麦麦:  {coordinateComponent.coordinate.DistanceTo(Player.Instance.Coordinate):0.0}");
                break;
            case EntityComponent entityComponent:
                slider.SetValue(entityComponent.health, entityComponent.maxHealth);
                slider.tipController.SetTip($"生命值:  {entityComponent.health}/{entityComponent.maxHealth}", slider.fillColor);
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
    /// 显示卡牌状态
    /// </summary>
    /// <param name="state"></param>
    private void DisplayCardState(Card card, CardState state)
    {
        if (state.needElectricity)
        {
            iconLayout.SetActive(true);
            flashIcon.gameObject.SetActive(true);
            flashIcon.color = state.isConsumingElectricity ? ColorManager.Yellow : ColorManager.DarkGrey;
        }

        // 有动画的播放动画
        if (state.isAnim)
        {
            cardAnimator.enabled = true;
            cardAnimator.Play(card.CardId + state.name);
        }
        else if (cardAnimator.enabled)
        {
            cardAnimator.Play("");
            cardAnimator.enabled = false;
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
            DisplayContinuousValueComponent(d, middle);
        // 显示新鲜度
        if (card.TryGetComponent<FreshnessComponent>(out var f))
            DisplayContinuousValueComponent(f, middle);
        // 显示生长度
        if (card.TryGetComponent<GrowthComponent>(out var g))
            DisplayContinuousValueComponent(g, middle);
        // 显示产物进度
        if (card.TryGetComponent<ProgressComponent>(out var p))
            DisplayContinuousValueComponent(p, middle);
        // 显示内容物数量
        if (card.TryGetComponent<InnerContentsComponent>(out var i))
            DisplayInnerContentsComponent(i);
        // 显示燃料存储
        if (card.TryGetComponent<FuelStorageComponent>(out var fc))
            DisplayContinuousValueComponent(fc, middle);
        // 显示温度
        if (card.TryGetComponent<TemperatureComponent>(out var t))
            DisplayContinuousValueComponent(t, right);
        // 显示状态
        if (card.TryGetComponent<StateMachineComponent>(out var s))
            DisplayCardState(card, s.CurrentState);
        // 显示氧气存储
        if (card.TryGetComponent<OxygenStorageComponent>(out var o))
            DisplayContinuousValueComponent(o, middle);
        // 显示盐水存储
        if (card.TryGetComponent<SalineWaterStorageComponent>(out var sw))
            DisplayContinuousValueComponent(sw, left, true);
        // 显示淡水存储
        if (card.TryGetComponent<FreshWaterStorageComponent>(out var fw))
            DisplayContinuousValueComponent(fw, right, true);
        // 显示计时器
        if (card.TryGetComponent<TimerComponent>(out var tm))
            DisplayContinuousValueComponent(tm, left);
        else if (componentSliders.TryGetValue(typeof(TimerComponent), out var timer)) // 因为计时器可能被移除，所以这里要检查一下，如果有就移除
        {
            componentSliders.Remove(typeof(TimerComponent));
            ObjectBufferPool.Instance.Restore(timer.gameObject);
        }
        // 显示植物生长度
        if (card.TryGetComponent<PlantGrowthComponent>(out var pg))
            DisplayContinuousValueComponent(pg, middle);
        // 显示坐标
        if (card.TryGetComponent<CoordinateComponent>(out var cc))
            DisplayContinuousValueComponent(cc, middle);
        // 显示实体生命值
        if (card.TryGetComponent<EntityComponent>(out var ec))
            DisplayContinuousValueComponent(ec, middle);

        // 显示额外信息
        moreInfoText.text = card.ExtraInfo;
    }

    /// <summary>
    /// 不显示卡牌
    /// </summary>
    public void Clear()
    {
        cardAnimator.enabled = false;

        cardCanvasGroup.alpha = 0;
        cardCanvasGroup.blocksRaycasts = false;
        cardCanvasGroup.interactable = false;

        ObjectBufferPool.Instance.RestoreAllChildren(middle);
        ObjectBufferPool.Instance.RestoreAllChildren(left);
        ObjectBufferPool.Instance.RestoreAllChildren(right);

        if (mask != null) mask.SetActive(false);

        if (innerContentsComponent != null) innerContentsComponent.gameObject.SetActive(false);
        if (iconLayout != null) iconLayout.SetActive(false);
        if (fireIcon != null) fireIcon.gameObject.SetActive(false);
        if (flashIcon != null) flashIcon.gameObject.SetActive(false);

        componentSliders.Clear();
        lastComponentValues.Clear();

        tipController.enabled = false;
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

    /// <summary>
    /// 显示卡牌如耐久度变化、
    /// </summary>
    /// <param name="minute"></param>
    public void DisplayComponentValuesChange(Dictionary<Type, float> deltaValues)
    {
        List<(bool up, int level, Color color)> groups = new();

        foreach (var (type, deltaValue) in deltaValues)
        {
            if (ColorManager.CardComponentColors.TryGetValue(type, out var color))
                groups.Add((deltaValue > 0, CalcLevel(deltaValue), color));
        }

        MFXUtility.ShowArrows(particleDisplayRect, groups);
    }

    public void DisplayComponentValueChange(Type componentType, float value)
    {
        if (ColorManager.CardComponentColors.TryGetValue(componentType, out var color))
            MFXUtility.ShowArrows(particleDisplayRect, value > 0, CalcLevel(value), color);
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