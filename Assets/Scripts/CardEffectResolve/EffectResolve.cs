using UnityEngine;

public class EffectResolve:MonoBehaviour
{
    private static EffectResolve instance;
    public static EffectResolve Instance => instance;
    public void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void Resolve(Card card,CardEvent cardEvent)
    {
        //时间增加，随时间状态数值结算
        TimeManager.Instance.AddTime(cardEvent.Time);
        //每个独立事件结算（数值，发卡牌）
        if (cardEvent.GetType() == typeof(NormalCardEvent))
        {
            foreach (var VARIABLE in cardEvent.eventList)
            {
                if (VARIABLE.GetType() == typeof(PlaceDropEvent))
                {
                    //TODO：判断环境背包一次性掉落是否全部掉落，未全掉落时运行背包中的单次掉落逻辑
                }
                else
                {
                    VARIABLE.EventResolve();
                }
                
            }
        }
        //对被结算卡牌做耐久处理
        if (card.GetType() == typeof(ResourcePointCard))
        {
            ResourcePointCard resourcePointCard = (ResourcePointCard)card;
            resourcePointCard.curEndurance--;
            if (resourcePointCard.curEndurance <= 0)
            {
                //TODO：调用删除该卡牌方法
            }
        }
        else if (card.GetType() == typeof(ToolCard))
        {
            ToolCard toolCard = (ToolCard)card;
            toolCard.curEndurance--;
        }
    }
}