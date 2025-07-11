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
        // 玩家背包
        playerBag = FindObjectOfType<PlayerBag>(true);
        // 所有环境背包
        foreach (var bag in FindObjectsOfType<EnvironmentBag>(true))
        {
            environmentBags.Add(bag.PlaceData.placeType, bag);
        }
        // 当前环境背包
        curEnvironmentBag = environmentBags[GameDataManager.Instance.LastPlace];
        //Init();
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
    public void ResolveExplore()
    {
        CardEvent cardEvent = curEnvironmentBag.CardEvent;
        foreach (var EventTrigger in cardEvent.eventList)
        {
            if (EventTrigger.GetType() == typeof(PlaceDropEvent))
            {
                EventTrigger.EventResolve();
            }
        }
    }




    // 判断事件执行条件
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

    // 处理卡牌事件
    public void Resolve(CardEvent cardEvent)
    {
        if (cardEvent == null) return;

        if (!ConditionEventJudge(cardEvent)) return;
        // 1. 处理玩家状态的数值变化
        foreach (var EventTrigger in cardEvent.eventList)
        {
            if (EventTrigger.GetType() == typeof(ValueEvent)) EventTrigger.EventResolve();
        }
        // 2. 处理场景移动
        foreach (var EventTrigger in cardEvent.eventList)
        {
            if (EventTrigger.GetType() == typeof(MoveEvent)) EventTrigger.EventResolve();
        }
        // 3. 处理时间变化
        TimeManager.Instance.AddTime(cardEvent.Time);
        // 4. 处理卡牌掉落
        foreach (var EventTrigger in cardEvent.eventList)
        {
            if (EventTrigger.GetType() == typeof(DropEvent)) EventTrigger.EventResolve();
        }
    }

    /// <summary>
    /// 掉落一张卡牌
    /// </summary>
    /// <param name="drop">掉落物的配置</param>
    /// <param name="ToPlayerBag">是否优先掉落到玩家背包</param>
    public void AddDropCard(Drop drop, bool ToPlayerBag)
    {
        for (int i = 0; i < drop.DropNum; i++)
        {
            // 处理空掉落
            if (drop.IsEmpty)
            {
                Debug.Log("什么也没有掉落");
                continue;
            }

            CardInstance cardInstance = CardFactory.CreateCardIntance(drop.GetCardData());
            if (ToPlayerBag)
            {
                // 
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
        foreach (var EventTrigger in curEnvironmentBag.CardEvent.eventList)
        {
            if (EventTrigger is PlaceDropEvent placeDropEvent)
            {
                placeDropEvent.DropCertainPlaceCard(lastPlace);
            }
        }

        
    }
}