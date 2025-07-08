using UnityEngine;

public class EffectResolve : MonoBehaviour
{
    private static EffectResolve instance;
    public static EffectResolve Instance => instance;
    [Header("玩家背包")]
    public PlayerBagCard PlayerBag;
    [Header("环境背包列表")]
    public EnvironmentBagCard[] EnvironmentBag;
    [Header("当前环境背包")]
    public EnvironmentBagCard CurEnvironmentBag;

    public void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ResolveExplore()
    {
        CardEvent cardEvent = CurEnvironmentBag.CardEvent;
        foreach (var EventTrigger in cardEvent.eventList)
        {
            //如果是探索事件进行特殊处理
            if (EventTrigger.GetType() == typeof(PlaceDropEvent))
            {
                //判断环境背包一次性掉落是否全部掉落，未全掉落时运行背包中的单次掉落逻辑
                if (CurEnvironmentBag.curOnceList.Count != 0)
                {
                    CurEnvironmentBag.ExploreEnv();
                }
                else
                {
                    EventTrigger.EventResolve();
                }
            }
        }
    }

    public void Resolve(Bag bag, Card card)
    {
        //遍历所有事件
        foreach (var cardEvent in card.cardData.cardEventList)
        {
            //时间增加，随时间状态数值结算
            TimeManager.Instance.AddTime(cardEvent.Time);
            //每个独立事件结算（数值，发卡牌），目前没有写需要条件触发的卡牌逻辑
            if (cardEvent.GetType() == typeof(NormalCardEvent))
            {
                foreach (var EventTrigger in cardEvent.eventList)
                {
                    //如果是探索事件进行特殊处理
                    if (EventTrigger.GetType() == typeof(PlaceDropEvent))
                    {
                        //判断环境背包一次性掉落是否全部掉落，未全掉落时运行背包中的单次掉落逻辑
                        if (CurEnvironmentBag.curOnceList.Count != 0)
                        {
                            CurEnvironmentBag.ExploreEnv();
                        }
                        else
                        {
                            EventTrigger.EventResolve();
                        }
                    }
                    else
                    {
                        //调用重复掉落逻辑
                        EventTrigger.EventResolve();
                    }
                }
            }

            //对被结算卡牌做耐久处理或移除被结算卡牌
            if (card.GetType() == typeof(ResourcePointCard))
            {
                ResourcePointCard resourcePointCard = (ResourcePointCard)card;
                resourcePointCard.curEndurance--;
                if (resourcePointCard.curEndurance <= 0)
                {
                    bag.RemoveCard(card);
                }
            }
            else if (card.GetType() == typeof(ToolCard))
            {
                ToolCard toolCard = (ToolCard)card;
                toolCard.curEndurance--;
                if (toolCard.curEndurance <= 0)
                {
                    bag.RemoveCard(card);
                }
            }
            else
            {
                bag.RemoveCard(card);
            }
        }
    }

    //掉落卡牌加入背包
    public void AddDropCard(Drop drop)
    {
        //判断背包格子数量是否已满
        if (PlayerBag.cardList.Count == InitPlayerStateData.Instance.maxPlayerGrid)
        {
            CurEnvironmentBag.AddCard(drop.cardData);
        }
        //判断背包重量是否会超过到达指定倍数
        else if (PlayerBag.curHeavy + drop.cardData.weight == InitPlayerStateData.Instance.maxWeightFactor *
                 InitPlayerStateData.Instance.maxPlayerWeight)
        {
            CurEnvironmentBag.AddCard(drop.cardData);
        }
        else
        {
            PlayerBag.AddCard(drop.cardData);
        }
    }
}