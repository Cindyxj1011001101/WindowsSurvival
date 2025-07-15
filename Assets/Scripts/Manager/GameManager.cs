using System.Collections.Generic;
using UnityEngine;

public enum PlaceEnum
{
    /// <summary>
    /// 动力舱
    /// </summary>
    PowerCabin,
    /// <summary>
    /// 驾驶室
    /// </summary>
    Cockpit,
    /// <summary>
    /// 维生舱
    /// </summary>
    LifeSupportCabin,
    /// <summary>
    /// 珊瑚礁海岸
    /// </summary>
    CoralCoast,

}

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance => instance;

    private PlayerBag playerBag;
    private Dictionary<PlaceEnum, EnvironmentBag> environmentBags = new();
    private EnvironmentBag curEnvironmentBag;
    private EquipmentBag equipmentBag;

    public PlayerBag PlayerBag => playerBag;
    public Dictionary<PlaceEnum, EnvironmentBag> EnvironmentBags => environmentBags;
    public EnvironmentBag CurEnvironmentBag => curEnvironmentBag;
    public EquipmentBag EquipmentBag => equipmentBag;

    public void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        // 玩家背包
        playerBag = FindObjectOfType<PlayerBag>(true);
        // 所有环境背包
        foreach (var bag in FindObjectsOfType<EnvironmentBag>(true))
        {
            environmentBags.Add(bag.PlaceData.placeType, bag);
        }
        // 当前环境背包
        curEnvironmentBag = environmentBags[GameDataManager.Instance.LastPlace];
        equipmentBag = FindObjectOfType<EquipmentBag>(true);
    }

    //初始化SO数据
    public void Init()
    {
        EventTrigger[] eventTriggers = Resources.LoadAll<EventTrigger>("ScriptableObject/EventTrigger");
        if (eventTriggers != null && eventTriggers.Length > 0)
        {
            foreach (var trigger in eventTriggers)
            {
                trigger.Init();
            }
        }
    }

    // 处理场景探索
    public void HandleExplore()
    {
        CardEvent cardEvent = curEnvironmentBag.ExploreEvent;
        foreach (var EventTrigger in cardEvent.eventList)
        {
            if (EventTrigger.GetType() == typeof(PlaceDropEvent))
            {
                EventTrigger.Invoke();
            }
        }
        // 处理时间变化
        TimeManager.Instance.AddTime(cardEvent.Time);
    }

    // 判断事件执行条件
    public bool CanCardEventInvoke(CardEvent cardEvent)
    {
        if (cardEvent.GetType() == typeof(ConditionalCardEvent))
        {
            ConditionalCardEvent conditionCardEvent = cardEvent as ConditionalCardEvent;
            foreach (var conditionData in conditionCardEvent.ConditionCardList)
            {
                CardSlot slot = playerBag.TryGetCardByCondition(conditionData);
                if (slot == null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    // 处理卡牌事件
    public void HandleCardEvent(CardEvent cardEvent)
    {
        if (cardEvent == null) return;
        // 数值变化-场景移动-时间变化-卡牌掉落

        if (!CanCardEventInvoke(cardEvent)) return;
        // 1. 处理玩家状态的数值变化
        foreach (var EventTrigger in cardEvent.eventList)
        {
            if (EventTrigger.GetType() == typeof(ValueEvent)) EventTrigger.Invoke();
        }
        // 2. 处理场景移动
        foreach (var EventTrigger in cardEvent.eventList)
        {
            if (EventTrigger.GetType() == typeof(MoveEvent)) EventTrigger.Invoke();
        }
        // 3. 处理时间变化
        TimeManager.Instance.AddTime(cardEvent.Time);
        // 4. 处理卡牌掉落
        foreach (var EventTrigger in cardEvent.eventList)
        {
            if (EventTrigger.GetType() == typeof(DropEvent)) EventTrigger.Invoke();
        }
    }

    /// <summary>
    /// 掉落一张卡牌
    /// </summary>
    /// <param name="drop">掉落物的配置</param>
    /// <param name="toPlayerBag">是否优先掉落到玩家背包</param>
    public void AddDropCard(Drop drop, bool toPlayerBag)
    {
        for (int i = 0; i < drop.DropNum; i++)
        {
            // 处理空掉落
            if (drop.IsEmpty)
            {
                Debug.Log("什么也没有掉落");
                continue;
            }

            AddCard(drop.card, toPlayerBag);
        }
    }

    public void AddCard(Card card, bool toPlayerBag)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("抽卡");
        CardInstance cardInstance = CardFactory.CreateCardIntance(card);
        if (toPlayerBag)
        {
            if (playerBag.CanAddCard(cardInstance))
                playerBag.AddCard(cardInstance);
            else
                curEnvironmentBag.AddCard(cardInstance);
        }
        else
        {
            curEnvironmentBag.AddCard(cardInstance);
        }
    }

    // 移动到目标场景
    public void Move(PlaceEnum targetPlace)
    {
        //拿到原先场景是哪个
        PlaceEnum lastPlace = curEnvironmentBag.PlaceData.placeType;

        foreach (var (place, bag) in environmentBags)
        {
            bag.gameObject.SetActive(place == targetPlace);
        }
        curEnvironmentBag = environmentBags[targetPlace];

        EventManager.Instance.TriggerEvent(EventType.Move, curEnvironmentBag);

        //从切换后的场景单次探索列表中拿出必定回到原先场景的牌，加入当前场景背包
        foreach (var EventTrigger in curEnvironmentBag.ExploreEvent.eventList)
        {
            if (EventTrigger is PlaceDropEvent placeDropEvent)
            {
                placeDropEvent.DropCertainPlaceCard(lastPlace);
            }
        }
    }
}