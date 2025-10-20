using Newtonsoft.Json;
using System.Collections.Generic;

public class DropConfig
{
    public string cardId;
    public int dropNum;
    public bool randomDropNum;
    public int minRandomDropNum;
    public int maxRandomDropNum;

    public DropConfig() { }

    public DropConfig(string cardId, int dropNum)
    {
        this.cardId = cardId;
        this.dropNum = dropNum;
        this.randomDropNum = false;
        this.minRandomDropNum = 0;
        this.maxRandomDropNum = 0;
    }

    public DropConfig(string cardId, int minRandomDropNum, int maxRandomDropNum)
    {
        this.cardId = cardId;
        this.dropNum = 0;
        this.randomDropNum = true;
        this.minRandomDropNum = minRandomDropNum;
        this.maxRandomDropNum = maxRandomDropNum;
    }

    public List<Card> GetDroppedCards()
    {
        List<Card> droppedCards = new();

        if (string.IsNullOrEmpty(cardId)) return droppedCards;

        int actualDropNum = dropNum;
        if (randomDropNum)
        {
            actualDropNum = UnityEngine.Random.Range(minRandomDropNum, maxRandomDropNum + 1);
        }
        for (int i = 0; i < actualDropNum; i++)
        {
            droppedCards.Add(CardFactory.CreateCard(cardId));
        }
        return droppedCards;
    }
}

[System.Serializable]
public class Drop
{
    public List<DropConfig> dropConfig = new(); // 掉落配置(卡牌id，数量)
    public int dropWeight; // 掉落权重

    public List<Card> droppedCards = new(); // 直接配置要掉落的卡牌

    [JsonIgnore] public OutStringAction onDrop;

    public Drop() { }

    public Drop(int dropWeight, params (string cardId, int dropNum)[] config)
    {
        foreach (var (cardId, dropNum) in config)
            dropConfig.Add(new(cardId, dropNum));

        this.dropWeight = dropWeight;
    }

    public Drop(int dropWeight, string cardId, int dropNum)
    {
        dropConfig.Add(new(cardId, dropNum));
        this.dropWeight = dropWeight;
    }

    public Drop(int dropWeight, string cardId, int minRandomDropNum, int maxRandomDropNum)
    {
        dropConfig.Add(new(cardId, minRandomDropNum, maxRandomDropNum));
        this.dropWeight = dropWeight;
    }

    public Drop(int dropWeight, List<DropConfig> dropConfig)
    {
        this.dropConfig = dropConfig;
        this.dropWeight = dropWeight;
    }

    public Drop(int dropWeight, params DropConfig[] dropConfig)
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
        foreach (var config in dropConfig)
        {
            droppedCards.AddRange(config.GetDroppedCards());
        }

        onDrop?.Invoke(out tip);

        return droppedCards;
    }
}