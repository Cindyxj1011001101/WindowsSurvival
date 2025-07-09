using System;
using UnityEngine;
public enum PlaceEnum
{
    //动力舱、驾驶室、维生舱
    EngineCompartment,
    Cab,
    LifeSupport

}
public class EffectResolve : MonoBehaviour
{
    private static EffectResolve instance;
    public static EffectResolve Instance => instance;
    [Header("玩家背包")]
    //public PlayerBag PlayerBag;
    public PlayerBagWindow playerBag;
    [Header("环境背包列表")]
    //public EnvironmentBag[] EnvironmentBag;
    public EnvironmentBagWindow[] environmentBags;
    [Header("当前环境背包")]
    //public EnvironmentBag CurEnvironmentBag;
    public EnvironmentBagWindow curEnvironmentBag;

    public void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
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
                curEnvironmentBag.AddCard(cardInstance);
            }
            else
            {
                playerBag.AddCard(cardInstance);
            }
        }
        else
        {
            curEnvironmentBag.AddCard(cardInstance);
        }
    }

    //场景移动
    public void Move(PlaceEnum aimPlace)
    {
        foreach (var environmentBag in environmentBags)
        {
            if (environmentBag.place == aimPlace)
            {
                curEnvironmentBag = environmentBag;
                EventManager.Instance.TriggerEvent(EventType.Move, curEnvironmentBag);
            }
        }
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