using System.Collections.Generic;
using UnityEngine;

public enum EventTypeEnum
{
    CanTriggerEvent,// 判断某一卡牌行为是否可执行
    TriggerEvent,//执行某一卡牌行为
    //获取某一事件参数
    //获取卡牌参数
    Fresh,//卡牌腐烂
    Endurance,//卡牌损坏
}
//水瓶鱼
public class AquariusFish:Card
{
    public int maxProductProcess;//最大生产进度
    public int curProductProcess;//当前生产进度
    public int maxProductNum;//最大生产数量
    public int curProductNum;//当前生产数量

    //构造函数(可增加多参构造实现非满新鲜与耐久的卡牌初始化)
    public AquariusFish()
    {
        //初始化参数
        cardName = "水瓶鱼";
        cardDesc = "水瓶鱼是白塔星浅海特有的卵胎生鱼类，其雄鱼体型不足雌鱼0.1% ，终生附着在雌鱼泄殖腔附近。怀孕期间，雌鱼通过腹腔生物渗透膜从海水中过滤淡水，混合蛋白质形成富含营养的琥珀色育卵液。其半透明腹腔可见游动的胚胎群。";
        cardImage = Resources.Load<Sprite>("CardImage/水瓶鱼");
        cardType = CardType.Creature;
        maxStackNum = 5;
        moveable = false;
        maxProductProcess = 5760;
        curProductProcess = 0;
        maxProductNum = 1;
        curProductNum = 0;
        weight = 1.1f;
        events = new List<Event>();
        tags = new List<CardTag>();
        events.Add(new Event("用捕网捉", "用捕网捉水瓶鱼", Event_CatchByNet, () => Judge_CatchByNet()));
    }
    public AquariusFish(int curProductProcess,int curProductNum)
    {
        //初始化参数
        cardName = "水瓶鱼";
        cardDesc = "水瓶鱼是白塔星浅海特有的卵胎生鱼类，其雄鱼体型不足雌鱼0.1% ，终生附着在雌鱼泄殖腔附近。怀孕期间，雌鱼通过腹腔生物渗透膜从海水中过滤淡水，混合蛋白质形成富含营养的琥珀色育卵液。其半透明腹腔可见游动的胚胎群。";
        cardImage = Resources.Load<Sprite>("CardImage/水瓶鱼");
        cardType = CardType.Creature;
        maxStackNum = 5;
        moveable = false;
        maxProductProcess = 5760;
        this.curProductProcess = curProductProcess;
        maxProductNum = 1;
        this.curProductNum = curProductNum;
        weight = 1.1f;
        events = new List<Event>();
        tags = new List<CardTag>();
        events.Add(new Event("用捕网捉", "用捕网捉水瓶鱼", Event_CatchByNet, () => Judge_CatchByNet()));
    }

    #region 用捕网捉
    public void Event_CatchByNet()
    {
        //消耗15分钟
        TimeManager.Instance.AddTime(30);
        //获得一张“被捉住的水瓶鱼”
        GameManager.Instance.AddCard(new CaughtAquariusFish(), true);
        //“捕网”耐久-1
        FindNet()?.Use();
        //TODO:删除本卡牌
        return;
    }

    public bool Judge_CatchByNet()
    {
        if(FindNet()!=null)
        {
            return true;
        }
        return false;
    }
    public Card FindNet()
    {
        List<CardSlot> cards =GameManager.Instance.CurEnvironmentBag.Slots;
        foreach(CardSlot slot in cards)
        {
            if(slot.card.cardName=="捕网")
            {
                return slot.card;
            }
        }
        return null;
    }
    #endregion
    #region 用手捉
    public void Event_CatchByHand()
    {
        int rand = Random.Range(0,4);
        if(rand<3)
        {
            GameManager.Instance.AddCard(null, true);
            StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.San, -2));
            EnvironmentBag environmentBag = GameManager.Instance.CurEnvironmentBag;        
            var e = environmentBag.ExploreEvent.eventList.Find(c => c is PlaceDropEvent);
            (e as PlaceDropEvent).curOnceDropList.Add(new Drop(new AquariusFish(), 1, "水瓶鱼"));
        }
        else
        {
            GameManager.Instance.AddCard(new CaughtAquariusFish(), true);
        }
        //TODO:删除本卡牌
        return;
    }
    public bool Judge_CatchByHand()
    {
        return true;
    }
    #endregion  
    public override void Use()
    {
        return;
    }

    public override void Grow()
    {
        if(curProductNum==maxProductNum)
        {
            return;
        }
        curProductProcess+=15;
        if(curProductProcess>=maxProductProcess)
        {
            curProductProcess=0;
            curProductNum+=1;
        }
    }

    public override void Fresh()
    {
        return;
    }
}