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
    LifeSupportCabin

}
public class EffectResolve : MonoBehaviour
{
    private static EffectResolve instance;
    public static EffectResolve Instance => instance;

    private PlayerBag playerBag;
    private Dictionary<PlaceEnum, EnvironmentBag> environmentBags = new();
    private EnvironmentBag curEnvironmentBag;

    public PlayerBag PlayerBag => playerBag;
    public Dictionary<PlaceEnum, EnvironmentBag> EnvironmentBags => environmentBags;
    public EnvironmentBag CurEnvironmentBag => curEnvironmentBag;

    public void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        // 记录玩家背包
        playerBag = FindObjectOfType<PlayerBag>(true);
        // 记录所有环境背包
        foreach (var bag in FindObjectsOfType<EnvironmentBag>(true))
        {
            environmentBags.Add(bag.PlaceData.placeType, bag);
        }
        // 当前环境背包
        curEnvironmentBag = environmentBags[GameDataManager.Instance.LastPlace];
    }

    //探索方法
    public void ResolveExplore()
    {
        CardEvent cardEvent = curEnvironmentBag.CardEvent;
        foreach (var EventTrigger in cardEvent.eventList)
        {
            //如果是探索事件进行特殊处理
            if (EventTrigger.GetType() == typeof(PlaceDropEvent))
            {
                EventTrigger.EventResolve();
            }
        }
    }

    //判断是否满足事件触发条件
    public bool ConditionEventJudge(CardEvent cardEvent)
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

    //点击卡牌事件触发方法
    public void Resolve(CardEvent cardEvent)
    {
        if (cardEvent == null) return;

        if (!ConditionEventJudge(cardEvent)) return;
        //状态结算
        foreach (var EventTrigger in cardEvent.eventList)
        {
            if (EventTrigger.GetType() == typeof(ValueEvent)) EventTrigger.EventResolve();
        }
        //场景切换
        foreach (var EventTrigger in cardEvent.eventList)
        {
            if (EventTrigger.GetType() == typeof(MoveEvent)) EventTrigger.EventResolve();
        }
        //时间流逝
        TimeManager.Instance.AddTime(cardEvent.Time);
        //掉落卡片
        foreach (var EventTrigger in cardEvent.eventList)
        {
            if (EventTrigger.GetType() == typeof(DropEvent)) EventTrigger.EventResolve();
        }
    }

    //掉落卡牌加入背包
    public void AddDropCard(Drop drop, bool ToPlayerBag)
    {
        CardInstance cardInstance = CardFactory.CreateCardIntance(drop.cardData);
        if (ToPlayerBag)
        {
            //判断背包格子数量是否已满
            if (playerBag.CanAddCard(cardInstance))
            {
                playerBag.AddCard(cardInstance);
            }
            else
            {
                curEnvironmentBag.AddCard(cardInstance);
            }
        }
        else
        {
            curEnvironmentBag.AddCard(cardInstance);
        }
    }

    //场景移动
    public void Move(PlaceEnum targetPlace)
    {
        foreach (var (place, bag) in environmentBags)
        {
            bag.gameObject.SetActive(place == targetPlace);
        }
        curEnvironmentBag = environmentBags[targetPlace];

        EventManager.Instance.TriggerEvent(EventType.Move, curEnvironmentBag);
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
}