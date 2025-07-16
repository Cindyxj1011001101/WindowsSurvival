using System.Collections.Generic;
using UnityEngine;

//老鼠尸体
public class RatBody:Card
{
    public int maxFresh;//最大新鲜度
    public int curFresh;//当前新鲜度
    public RatBody()
    {
        //初始化参数
        cardName = "老鼠尸体";
        cardDesc = "一只老鼠的尸体，可以用来制作食物。";
        cardImage = Resources.Load<Sprite>("CardImage/老鼠尸体");
        cardType = CardType.Food;
        maxStackNum =1;
        maxFresh=2880;
        curFresh=2880;
        moveable = true;
        weight = 0.7f;
        events = new List<Event>();
        events.Add(new Event("食用", "食用老鼠尸体", Event_Eat, () => Judge_Eat()));
        events.Add(new Event("用手剥", "用手剥老鼠尸体", Event_PeelByHand, () => Judge_PeelByHand()));
        events.Add(new Event("用刀切割", "用刀切割老鼠尸体", Event_PeelByKnife, () => Judge_PeelByKnife()));
    }

    //刷新耐久度
    public override void Fresh()
    {
        curFresh--;
        if(curFresh<=0)
        {
            //TODO:删除本卡牌
            //TODO:掉落新卡牌
            GameManager.Instance.AddCard(new RotMaterial(), true);
        }
    }
#region 食用
    public void Event_Eat()
    {
        //+16饱食
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Fullness, 16));
        //-20精神值
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.San, -20));
        //-8健康
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Health, -8));
        //消耗30分钟
        TimeManager.Instance.AddTime(30);
        //销毁老鼠尸体
        //TODO:删除本卡牌
    }

    public bool Judge_Eat()
    {
        return true;
    }
#endregion
#region 用手剥
    public void Event_PeelByHand()
    {
        //-3精神值
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.San, -3));
        //-2健康
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Health, -2));
        //消耗45分钟
        TimeManager.Instance.AddTime(45);
        //随机掉落卡牌
        RandomDrop();
        //销毁老鼠尸体
        //TODO:删除本卡牌
    }

    public bool Judge_PeelByHand()
    {
        return true;
    }
#endregion
#region 用刀切割
    public void Event_PeelByKnife()
    {
        //消耗15分钟
        TimeManager.Instance.AddTime(15);
        GameManager.Instance.AddCard(new LittleRawMeat(), true);
        FindKnife()?.Use();
    }

    public bool Judge_PeelByKnife()
    {
        if(FindKnife()!=null)
        {
            return true;
        }
        return false;
    }

    public Card FindKnife()
    {
        List<CardSlot> cards =GameManager.Instance.CurEnvironmentBag.Slots;
        foreach(CardSlot slot in cards)
        {
            if(slot.card.cardType==CardType.Tool)
            {
                if(slot.card.tags.Contains(CardTag.Cut))
                {
                    return slot.card;
                }
            }
        }
        return null;
    }
#endregion

#region 随机掉落
    public void RandomDrop()
    {
        int rand = Random.Range(0, 4);
        if(rand<3)
        {
            GameManager.Instance.AddCard(new LittleRawMeat(), true);
        }
        else if(rand<4)
        {
            GameManager.Instance.AddCard(null, true);
        }
    }
#endregion
}