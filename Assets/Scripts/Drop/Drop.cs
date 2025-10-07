using Newtonsoft.Json;
using System.Collections.Generic;

[System.Serializable]
public class Drop
{
    public List<(string cardId, int dropNum)> dropConfig = new(); // 掉落配置(卡牌id，数量)
    public int dropWeight; // 掉落权重

    public List<Card> droppedCards = new(); // 已掉落的卡牌

    [JsonIgnore] public OutStringAction onDrop;

    public Drop() { }

    public Drop(int dropWeight, List<(string cardId, int dropNum)> dropConfig)
    {
        this.dropConfig = dropConfig;
        this.dropWeight = dropWeight;
    }

    public Drop(int dropWeight, params (string cardId, int dropNum)[] dropConfig)
    {
        this.dropConfig = new(dropConfig);
        this.dropWeight = dropWeight;
    }

    public Drop(int dropWeight, List<Card> droppedCards)
    {
        this.dropWeight = dropWeight;
        this.droppedCards = droppedCards;
    }

    public Drop(int dropWeight, OutStringAction onDrop)
    {
        this.dropWeight = dropWeight;
        this.onDrop = onDrop;
    }

    public List<Card> GetDroppedCards(out string tip)
    {
        tip = string.Empty;

        if (!droppedCards.IsNullOrEmpty()) return droppedCards;

        droppedCards = new();
        foreach (var (cardId, dropNum) in dropConfig)
        {
            if (string.IsNullOrEmpty(cardId) || dropNum <= 0) continue;

            for (int i = 0; i < dropNum; i++)
            {
                droppedCards.Add(CardFactory.CreateCard(cardId));
            }
        }

        onDrop?.Invoke(out tip);

        return droppedCards;
    }
}