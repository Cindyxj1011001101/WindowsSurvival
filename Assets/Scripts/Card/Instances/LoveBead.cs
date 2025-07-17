using System.Collections.Generic;
using UnityEngine;

public class LoveBead : Card
{
    public int maxProductProcess;//最大生产进度
    public int curProductProcess;//当前生产进度
    public int maxProductNum;//最大生产数量
    public int curProductNum;//当前生产数量
    public int maxFresh;//最大新鲜度
    public int curFresh;//当前新鲜度
    public bool isOpened;//是否已撬开

    public LoveBead()
    {
        cardName = "爱情贝";
        cardDesc = "形似扇贝的古怪异星生物。其强大的肌肉使其能像鱼一样扇动贝壳游动。繁殖期的雄贝会搜集各类珍宝藏在贝壳中以吸引雌贝。";
        cardImage = Resources.Load<Sprite>("CardImage/爱情贝");
        cardType = CardType.Creature;
        maxStackNum = 5;
        moveable = true;
        maxProductProcess = 900;
        curProductProcess = 0;
        maxProductNum = 1;
        curProductNum = 0;
        maxFresh = 2160;
        curFresh = 2160;
        weight = 1.5f;
        isOpened = false;
        events = new List<Event>();
        tags = new List<CardTag>();
        events.Add(new Event("撬开", "撬开爱情贝", Event_OpenByTool, () => Judge_OpenByTool()));
        events.Add(new Event("取贝肉", "取贝肉", Event_GetMeat, () => Judge_GetMeat()));
    }

    public void Event_OpenByTool()
    {
        Card tool = FindTool();
        if (tool != null && curProductNum > 0 && !isOpened)
        {
            tool.Use();
            isOpened = true;
            TimeManager.Instance.AddTime(15);
            //撬开概率
            int random = Random.Range(0, 15);
            if (random < 3)
            {
                //GameManager.Instance.AddCard(new 玻璃沙, true);
                //GameManager.Instance.AddCard(new 玻璃沙, true);
            }
            else if (random < 6)
            {
                GameManager.Instance.AddCard(new ScrapMetal(), true);
                GameManager.Instance.AddCard(new ScrapMetal(), true);
            }
            else if (random < 9)
            {
                //GameManager.Instance.AddCard(new 珊瑚, true);
            }
            else if (random < 12)
            {
                GameManager.Instance.AddCard(new HardFiber(), true);
            }
            else if (random < 15)
            {
                //GameManager.Instance.AddCard(new 白爆矿, true);
            }
            //产物消耗
            curProductNum--;
        }
    }

    public bool Judge_OpenByTool()
    {
        Card tool = FindTool();
        if (tool != null && curProductNum > 0 && !isOpened)
        {
            return true;
        }
        return false;
    }

    public Card FindTool()
    {
        EnvironmentBag environmentBag = GameManager.Instance.CurEnvironmentBag;
        foreach (CardSlot slot in environmentBag.Slots)
        {
            if (slot.card.tags.Contains(CardTag.Cut) || slot.card.tags.Contains(CardTag.Dig))
            {
                return slot.card;
            }
        }
        return null;
    }

    public void Event_GetMeat()
    {
        if (isOpened)
        {
            TimeManager.Instance.AddTime(30);
            //GameManager.Instance.AddCard(new 生贝肉, true);
            //TODO:删除本卡牌
        }
    }

    public bool Judge_GetMeat()
    {
        if (isOpened)
        {
            return true;
        }
        return false;
    }

    public override void Grow()
    {
        if (curProductNum == maxProductNum)
        {
            return;
        }
        curProductProcess += 15;
        if (curProductProcess >= maxProductProcess)
        {
            curProductProcess = 0;
            curProductNum += 1;
        }
    }

    public override void Fresh()
    {
        //TODO:只有在玩家背包或非水域环境会掉新鲜度，一旦回到水域环境中会回满新鲜度
        //需要做成两张卡牌分别在背包和环境中
        //TODO:腐烂爱情贝
    }

}