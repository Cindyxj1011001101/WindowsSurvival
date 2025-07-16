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

    public void AddCard(Card card, bool toPlayerBag)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("抽卡", true);

        // 卡牌的属性开始随时间变化
        card.StartUpdating();
        if (toPlayerBag)
        {
            if (playerBag.CanAddCard(card))
                playerBag.AddCard(card);
            else
                curEnvironmentBag.AddCard(card);
        }
        else
        {
            curEnvironmentBag.AddCard(card);
        }
    }

    public void AddCard(string cardName, bool toPlayerBag)
    {
        var card = CardFactory.CreateCard(cardName);
        if (card != null)
            AddCard(card, toPlayerBag);
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