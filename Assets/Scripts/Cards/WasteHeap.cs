using System.Collections.Generic;
using UnityEngine;

//安全泡沫覆盖的废料堆
public class WasteHeap:Card
{
    public int maxEndurance;//最大耐久度
    public int curEndurance;//当前耐久度
    private List<Drop> dropCards;

    //构造函数(可增加多参构造实现非满新鲜与耐久的卡牌初始化)
    public WasteHeap()
    {
        //初始化参数
        cardName = "安全泡沫覆盖的废料堆";
        cardDesc = "安全泡沫覆盖的废料堆。";
        cardImage = Resources.Load<Sprite>("CardImage/安全泡沫覆盖的废料堆");
        cardType = CardType.ResourcePoint;
        maxStackNum =1;
        moveable = false;
        weight = 0.3f;
        maxEndurance = 5;
        curEndurance = 5;
        events = new List<Event>();
        events.Add(new Event("挖掘", "挖掘废料堆", Event_Dig, () => Judge_Dig()));
    }

    public void Event_Dig()
    {
        //消耗45分钟
        TimeManager.Instance.AddTime(1);
        //消耗1点耐久度
        Use();
        //掉落卡牌
        RandomDrop();

    }

    public bool Judge_Dig()
    {
        return true;
    }

    public override void Use()
    {
        curEndurance--;
        if (curEndurance <= 0)
        {
            //TODO:删除本卡牌
        }
    }

    public void RandomDrop()
    {
        //TODO:掉落卡牌逻辑
        int rand = Random.Range(0, 20);
        if(rand<5)
        {
            GameManager.Instance.AddCard(new ScrapMetal(), true);
            GameManager.Instance.AddCard(new ScrapMetal(), true);
        }
        else if(rand<9)
        {
            GameManager.Instance.AddCard(new ScrapMetal(), true);
        }
        else if(rand<13)
        {
            GameManager.Instance.AddCard(new HardFiber(), true);
        }
        else if(rand<16)
        {
            GameManager.Instance.AddCard(new CompactBiscuit(), true);
        }
        else if(rand<17)
        {
            GameManager.Instance.AddCard(new RatBody(), true);
        }
        else if(rand<18)
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