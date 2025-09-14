using Newtonsoft.Json;
using System.Collections.Generic;

[System.Serializable]
public class Drop
{
    public Card card; // 卡牌
    public string cardId; // 卡牌id
    public int dropNum; // 掉落数量
    public int dropProb; // 掉落概率，是正整数，dropProb除以一个掉落列表中所有地dropProb之和为真实掉落概率

    [JsonIgnore] public OutStringAction onDropped;

    public Drop() { }

    public Drop(Card card, int dropNum, int dropProb, OutStringAction onDropped = null)
    {
        this.card = card;
        this.dropNum = dropNum;
        this.dropProb = dropProb;
        this.onDropped = onDropped;
    }

    public Drop(string cardId, int dropNum, int dropProb, OutStringAction onDropped = null)
    {
        this.cardId = cardId;
        this.dropNum = dropNum;
        this.dropProb = dropProb;
        this.onDropped = onDropped;
    }

    public Drop(int dropProb, OutStringAction onDropped)
    {
        this.dropProb = dropProb;
        this.onDropped = onDropped;
    }

    public List<Card> GetDroppedCards(out string tip)
    {
        tip = string.Empty;

        List<Card> droppedCards = new();
        // 创建卡牌
        for (int i = 0; i < dropNum; i++)
        {
            Card toDrop = null;
            
            if (card != null)
                toDrop = JsonManager.DeepCopy(card);
            else if (!string.IsNullOrEmpty(cardId))
                toDrop = CardFactory.CreateCard(cardId);

            if (toDrop == null) continue;

            // 深拷贝
            droppedCards.Add(toDrop);
        }

        onDropped?.Invoke(out tip);

        return droppedCards;
    }
}