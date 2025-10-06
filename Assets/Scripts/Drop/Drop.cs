using Newtonsoft.Json;
using System.Collections.Generic;

[System.Serializable]
public class Drop
{
    public List<(string cardId, int dropNum)> dropConfig = new(); // 掉落配置(卡牌id，数量)
    public int dropProb;

    public List<Card> droppedCards = new(); // 已掉落的卡牌

    [JsonIgnore] public OutStringAction onDrop;

    public Drop() { }

    public Drop(int dropProb, List<(string cardId, int dropNum)> dropConfig)
    {
        this.dropConfig = dropConfig;
        this.dropProb = dropProb;
    }

    public Drop(int dropProb, params (string cardId, int dropNum)[] dropConfig)
    {
        this.dropConfig = new(dropConfig);
        this.dropProb = dropProb;
    }

    public Drop(int dropProb, List<Card> droppedCards)
    {
        this.dropProb = dropProb;
        this.droppedCards = droppedCards;
    }

    public Drop(int dropProb, OutStringAction onDrop)
    {
        this.dropProb = dropProb;
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