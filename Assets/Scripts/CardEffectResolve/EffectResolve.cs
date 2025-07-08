
using UnityEngine;

public class EffectResolve:MonoBehaviour
{
    private static EffectResolve instance;
    public static EffectResolve Instance => instance;
    public PlayerBag PlayerBag;
    public EnvironmentBag[] EnvironmentBag;
    public EnvironmentBag CurEnvironmentBag;
    public void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        EventManager.Instance.AddListener<Drop>(EventType.AddDropCard,AddDropCard);
    }
    public void OnDestroy()
    {
        EventManager.Instance.RemoveListener<Drop>(EventType.AddDropCard,AddDropCard);
    }
    public void Resolve(Bag bag, Card card)
    {
        //遍历所有事件
        foreach (var cardEvent in card.cardData.cardEventList)
        {
            //时间增加，随时间状态数值结算
                    TimeManager.Instance.AddTime(cardEvent.Time);
                    //每个独立事件结算（数值，发卡牌）
                    if (cardEvent.GetType() == typeof(NormalCardEvent))
                    {
                        foreach (var perEvent in cardEvent.eventList)
                        {
                            if (perEvent.GetType() == typeof(PlaceDropEvent))
                            {
                                //判断环境背包一次性掉落是否全部掉落，未全掉落时运行背包中的单次掉落逻辑
                                if (CurEnvironmentBag.curOnceList.Count != 0)
                                {
                                    CurEnvironmentBag.ExploreEnv();
                                }
                            }
                            else
                            {
                                //调用重复掉落逻辑
                                perEvent.EventResolve();
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
        else if(PlayerBag.curHeavy+drop.cardData.Weight == InitPlayerStateData.Instance.maxWeightFactor*InitPlayerStateData.Instance.maxPlayerWeight)
        {
            CurEnvironmentBag.AddCard(drop.cardData);
        }
        else
        {
            PlayerBag.AddCard(drop.cardData);
        }
        
    }
}