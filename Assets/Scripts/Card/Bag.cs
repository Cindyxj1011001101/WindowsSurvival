using System.Collections.Generic;
using UnityEngine;

public abstract class Bag
{
    public List<Card> cardList = new List<Card>();
    public abstract void Init();
    public abstract void AddCard(CardData cardData);
    public abstract void RemoveCard(Card card);
}

public class PlayerBag : Bag
{
    public PlayerBagData playerBagData;
    public float curHeavy;
    public int curStack;

    //初始化，将所有初始卡牌数据添加到卡牌列表中
    public override void Init()
    {
        //遍历需要增加的卡牌数据
        foreach (var cardData in playerBagData.cardList)
        {
            AddCard(cardData);
        }
    }

    //添加卡牌
    public override void AddCard(CardData cardData)
    {
        //记录当前是否有相同卡牌
            bool hasSame = false;
            //在当前卡牌库中寻找该卡牌
            foreach (var card in cardList)
            {
                //卡牌数据相同且未到达堆叠上限时找到相同可堆叠卡牌
                if (card.cardData == cardData&&card.cardNum<cardData.MaxStackNum)
                {
                    //堆叠该卡牌
                    card.cardNum++;
                    curHeavy+=cardData.Weight;
                    hasSame = true;
                    return;
                }
            }
            //没有相同卡牌则按照类型创建
            if (hasSame== false)
            {
                curStack++;
                //卡牌数据不同或者已到达堆叠上限
                //根据卡牌类型创建该卡牌
                if (cardData.GetType() == typeof(ResourcePointCardData))
                {
                    ResourcePointCardData resourcePointCard = (ResourcePointCardData)cardData;
                    cardList.Add(new ResourcePointCard(cardData,1,resourcePointCard.maxEndurance));
                }
                else if (cardData.GetType() == typeof(ToolCardData))
                {
                    ToolCardData resourcePointCard = (ToolCardData)cardData;
                    cardList.Add(new ToolCard(cardData,1,resourcePointCard.maxEndurance));
                }
                
            }
    }


    public override void RemoveCard(Card card)
    {
        curHeavy-=card.cardData.Weight;
        if (cardList.Contains(card))
        {
            if (card.cardNum > 1)
            {
                card.cardNum--;
                return;
            }
            else
            {
                cardList.Remove(card);
                curStack--;
            }
        }
        
        
    }
}
public class EnvironmentBag : Bag
{
    public CardEvent CardEvent;
    public float curDescoveryDegree;
    public bool ExploredAllOnce;
    public List<Drop> curOnceList;
    public override void Init()
    {
        //探索度归零
        curDescoveryDegree = 0;
        //获得地点掉落事件
        PlaceDropEvent placeDropEvent=CardEvent.eventList[0] as PlaceDropEvent;
        //重置单次掉落事件
        curOnceList = placeDropEvent.OnceDropList;
        //遍历需要增加的卡牌数据
        foreach (var cardDrop in placeDropEvent.DefaultList)
        {
            //记录当前是否有相同卡牌
            bool hasSame = false;
            //在当前卡牌库中寻找该卡牌
            foreach (var card in cardList)
            {
                //卡牌数据相同且未到达堆叠上限时找到相同可堆叠卡牌
                if (card.cardData == cardDrop.cardData&&card.cardNum<cardDrop.cardData.MaxStackNum)
                {
                    //堆叠该卡牌
                    card.cardNum++;
                    hasSame = true;
                    return;
                }
            }
            //没有相同卡牌则按照类型创建
            if (hasSame== false)
            {
                //卡牌数据不同或者已到达堆叠上限
                //根据卡牌类型创建该卡牌
                if (cardDrop.cardData.GetType() == typeof(ResourcePointCardData))
                {
                    ResourcePointCardData resourcePointCard = (ResourcePointCardData)cardDrop.cardData;
                    cardList.Add(new ResourcePointCard(cardDrop.cardData,1,resourcePointCard.maxEndurance));
                }
                else if (cardDrop.cardData.GetType() == typeof(ToolCardData))
                {
                    ToolCardData resourcePointCard = (ToolCardData)cardDrop.cardData;
                    cardList.Add(new ToolCard(cardDrop.cardData,1,resourcePointCard.maxEndurance));
                }
                
            }
            
        }
    }

    //单次掉落逻辑
    public void ExploreEnv()
    {
        if (curOnceList.Count != 0)
        {
            int sumProb=0;
            foreach (var drop in curOnceList)
            {
                sumProb += drop.DropProb;
            }
            int rand = Random.Range(0, sumProb);
            foreach (var drop in curOnceList)
            {
                if (rand < drop.DropProb)
                {
                    EventManager.Instance.TriggerEvent(EventType.AddDropCard, drop);
                    curOnceList.Remove(drop);
                    return;
                }
                rand -= drop.DropProb;
            }
        }
    }

    public override void AddCard(CardData cardData)
    {
        //记录当前是否有相同卡牌
        bool hasSame = false;
        //在当前卡牌库中寻找该卡牌
        foreach (var card in cardList)
        {
            //卡牌数据相同且未到达堆叠上限时找到相同可堆叠卡牌
            if (card.cardData == cardData&&card.cardNum<cardData.MaxStackNum)
            {
                //堆叠该卡牌
                card.cardNum++;
                hasSame = true;
                return;
            }
        }
        //没有相同卡牌则按照类型创建
        if (hasSame== false)
        {
            //卡牌数据不同或者已到达堆叠上限
            //根据卡牌类型创建该卡牌
            if (cardData.GetType() == typeof(ResourcePointCardData))
            {
                ResourcePointCardData resourcePointCard = (ResourcePointCardData)cardData;
                cardList.Add(new ResourcePointCard(cardData,1,resourcePointCard.maxEndurance));
            }
            else if (cardData.GetType() == typeof(ToolCardData))
            {
                ToolCardData resourcePointCard = (ToolCardData)cardData;
                cardList.Add(new ToolCard(cardData,1,resourcePointCard.maxEndurance));
            }
                
        }
    }
    public override void RemoveCard(Card card)
    {
        if (cardList.Contains(card))
        {
            if (card.cardNum > 1)
            {
                card.cardNum--;
            }
            else
            {
                cardList.Remove(card);
            }
        }
    }
}
