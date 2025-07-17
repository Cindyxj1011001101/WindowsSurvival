using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 被安全泡沫覆盖的废料堆
/// </summary>
public class WasteHeap : Card
{
    //private List<Drop> dropCards;

    public WasteHeap()
    {
        //初始化参数
        cardName = "废料堆";
        cardDesc = "被安全泡沫覆盖的废料堆。";
        //cardImage = Resources.Load<Sprite>("CardImage/安全泡沫覆盖的废料堆");
        cardType = CardType.ResourcePoint;
        maxStackNum = 1;
        moveable = false;
        weight = 0.3f;
        curEndurance = maxEndurance = 5;
        tags = new();
        events = new List<Event>
        {
            new Event("挖掘", "挖掘废料堆", Event_Dig, null)
        };
        components = new();
    }

    public void Event_Dig()
    {
        //消耗1点耐久度
        Use();
        //消耗45分钟
        TimeManager.Instance.AddTime(1);
        //掉落卡牌
        RandomDrop();
    }

    public void RandomDrop()
    {
        //TODO:掉落卡牌逻辑
        int rand = Random.Range(0, 20);
        if (rand < 5)
        {
            GameManager.Instance.AddCard(new ScrapMetal(), true);
            GameManager.Instance.AddCard(new ScrapMetal(), true);
        }
        else if (rand < 9)
        {
            GameManager.Instance.AddCard(new ScrapMetal(), true);
        }
        else if (rand < 13)
        {
            GameManager.Instance.AddCard(new HardFiber(), true);
        }
        else if (rand < 16)
        {
            GameManager.Instance.AddCard(new CompactBiscuit(), true);
        }
        else if (rand < 17)
        {
            GameManager.Instance.AddCard(new RatBody(), true);
        }
        else if (rand < 18)
        {
            GameManager.Instance.AddCard(new RotMaterial(), true);
        }
        else
        {
            //TODO:掉落氧烛
            //GameManager.Instance.AddCard(new 氧烛, true);
        }
    }
}