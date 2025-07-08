using System.Collections.Generic;
using UnityEngine;

public class EnvironmentBag : BagBase
{ 
    public CardEvent CardEvent;
    public PlaceEnum place;
    public float curDescoveryDegree;
    public override void AddCard(CardInstance card)
    {
        // 如果放不下，就新增格子
        if (!CanAddCard(card))
        {
            // 暂定每次新增3个格子
            AddSlot(3);
        }
        base.AddCard(card);
    }
    
    
    // public void AddCard(CardData cardData)
    // {
        // //记录当前是否有相同卡牌
        // bool hasSame = false;
        // //在当前卡牌库中寻找该卡牌
        // foreach (var card in cardList)
        // {
        //     //卡牌数据相同且未到达堆叠上限时找到相同可堆叠卡牌
        //     if (card.cardData == cardData&&card.cardNum<cardData.maxStackNum)
        //     {
        //         //堆叠该卡牌
        //         card.cardNum++;
        //         hasSame = true;
        //         return;
        //     }
        // }
        // //没有相同卡牌则按照类型创建
        // if (hasSame== false)
        // {
        //     //卡牌数据不同或者已到达堆叠上限
        //     //根据卡牌类型创建该卡牌
        //     if (cardData.GetType() == typeof(ResourcePointCardData))
        //     {
        //         ResourcePointCardData resourcePointCard = (ResourcePointCardData)cardData;
        //         cardList.Add(new ResourcePointCard(cardData,1,resourcePointCard.maxEndurance));
        //     }
        //     else if (cardData.GetType() == typeof(ToolCardData))
        //     {
        //         ToolCardData resourcePointCard = (ToolCardData)cardData;
        //         cardList.Add(new ToolCard(cardData,1,resourcePointCard.maxEndurance));
        //     }
        //          
        // }
   // }
}