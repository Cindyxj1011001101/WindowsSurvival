using System;
using UnityEngine;

[Serializable]
public class PlayerBagCard : Bag
{
    public PlayerBagData playerBagData;
    public float curHeavy;
    public int curStack;

    public PlayerBagCard()
    {
        curHeavy = 0;
        curStack = 0;
        Init();
    }
    //初始化，将所有初始卡牌数据添加到卡牌列表中
    public override void Init()
    {
        //遍历需要增加的卡牌数据
        if (playerBagData!=null&&playerBagData.cardList != null)
        {
            foreach (var cardData in playerBagData.cardList)
            {
                AddCard(cardData);
            }
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
                if (card.cardData == cardData&&card.cardNum<cardData.maxStackNum)
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
                else
                {
                    cardList.Add(new Card(cardData,1));
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