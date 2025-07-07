using System.Collections.Generic;

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
                    //食物，资源和工具卡牌增加重量
                    if (cardData.GetType() == typeof(FoodCardData))
                    {
                        FoodCardData FoodCard = (FoodCardData)cardData;
                        curHeavy+=FoodCard.Weight;
                    }
                    else if (cardData.GetType() == typeof(ToolCardData))
                    {
                        ToolCardData ToolCard = (ToolCardData)cardData;
                        curHeavy+=ToolCard.Weight;
                    }
                    else if (cardData.GetType() == typeof(ResourceCardData))
                    {
                        ResourceCardData ResourceCard = (ResourceCardData)cardData;
                        curHeavy+=ResourceCard.Weight;
                    }
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

    //TODO：移除单张卡牌，处理移除一堆卡牌中的一张
    public override void RemoveCard(Card card)
    {
        if (card.cardData.GetType() == typeof(FoodCardData))
        {
            FoodCardData FoodCard = (FoodCardData)card.cardData;
            curHeavy-=FoodCard.Weight;
        }
        else if (card.cardData.GetType() == typeof(ToolCardData))
        {
            ToolCardData ToolCard = (ToolCardData)card.cardData;
            curHeavy-=ToolCard.Weight;
        }
        else if (card.cardData.GetType() == typeof(ResourceCardData))
        {
            ResourceCardData ResourceCard = (ResourceCardData)card.cardData;
            curHeavy-=ResourceCard.Weight;
        }
        cardList.Remove(card);
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
        curDescoveryDegree = 0;
        PlaceDropEvent placeDropEvent=CardEvent.eventList[0] as PlaceDropEvent;
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
        cardList.Remove(card);
    }
}
