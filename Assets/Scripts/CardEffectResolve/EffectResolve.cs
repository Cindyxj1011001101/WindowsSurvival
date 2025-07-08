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
    public PlayerBag PlayerBag;
    [Header("环境背包列表")]
    public EnvironmentBag[] EnvironmentBag;
    [Header("当前环境背包")]
    public EnvironmentBag CurEnvironmentBag;

    public void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    //探索方法
    public void ResolveExplore()
    {
        CardEvent cardEvent = CurEnvironmentBag.CardEvent;
        foreach (var EventTrigger in cardEvent.eventList)
        {
            //如果是探索事件进行特殊处理
            if (EventTrigger.GetType() == typeof(PlaceDropEvent))
            {
                EventTrigger.EventResolve();
            }
        }
    }

    //点击卡牌事件触发方法
    public void Resolve(CardEvent cardEvent)
    {
        //状态结算
        foreach (var EventTrigger in cardEvent.eventList)
        {
            if(EventTrigger.GetType() == typeof(ValueEvent)) EventTrigger.EventResolve();
        }
        //场景切换
        foreach (var EventTrigger in cardEvent.eventList)
        {
            if(EventTrigger.GetType() == typeof(MoveEvent)) EventTrigger.EventResolve();
        }
        //时间流逝
        TimeManager.Instance.AddTime(cardEvent.Time);
        //掉落卡片
        foreach (var EventTrigger in cardEvent.eventList)
        {
            if(EventTrigger.GetType() == typeof(DropEvent)) EventTrigger.EventResolve();
        }
    }

    //掉落卡牌加入背包
    public void AddDropCard(Drop drop,bool ToPlayerBag)
    {
        if (ToPlayerBag)
        {
            //判断背包格子数量是否已满
            if (PlayerBag.slots.Count == InitPlayerStateData.Instance.maxPlayerGrid)
            {
                CurEnvironmentBag.AddCard(drop.cardData);
            }
            //判断背包重量是否会超过到达指定倍数
            else if (PlayerBag.CurrentLoad + drop.cardData.weight == InitPlayerStateData.Instance.maxWeightFactor *
                     InitPlayerStateData.Instance.maxPlayerWeight)
            {
                CurEnvironmentBag.AddCard(drop.cardData);
            }
        }
        else
        {
            PlayerBag.AddCard(drop.cardData);
        }
    }
    
    //场景移动
    public void Move(PlaceEnum aimPlace)
    {
        foreach (var environmentBag in EnvironmentBag)
        {
            if (environmentBag.place == aimPlace)
            {
                CurEnvironmentBag = environmentBag;
                //TODO：重载场景背包事件
            }
        }
    }
}