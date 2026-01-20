using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public class DropConfig
{
    public string cardId;
    public Card cardTemplate;
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

    public DropConfig(Card cardTemplate, int dropNum)
    {
        this.cardTemplate = cardTemplate;
        this.dropNum = dropNum;
        this.randomDropNum = false;
        this.minRandomDropNum = 0;
        this.maxRandomDropNum = 0;
    }

    public DropConfig(Card cardTemplate, int minRandomDropNum, int maxRandomDropNum)
    {
        this.cardTemplate = cardTemplate;
        this.dropNum = 0;
        this.randomDropNum = true;
        this.minRandomDropNum = minRandomDropNum;
        this.maxRandomDropNum = maxRandomDropNum;
    }

    public List<Card> GetDroppedCards()
    {
        if (cardTemplate == null && string.IsNullOrEmpty(cardId)) return new();

        var dropCards = new List<Card>();

        // 实际掉落数量
        int actualDropNum = randomDropNum ? UnityEngine.Random.Range(minRandomDropNum, maxRandomDropNum + 1) : dropNum;

        if (cardTemplate != null)
        {
            for (int i = 0; i < actualDropNum; i++)
            {
                dropCards.Add(CardFactory.DeepCopyCard(cardTemplate));
            }
        }
        else
        {
            dropCards = CardFactory.CreateCards(cardId, actualDropNum);
        }

        return dropCards;
    }

    public bool ContainsCard(string cardId)
    {
        return this.cardId == cardId || cardTemplate?.CardId == cardId;
    }
}

[System.Serializable]
public class Drop
{
    public List<DropConfig> dropConfig = new(); // 掉落配置(卡牌id，数量)
    public int dropWeight; // 掉落权重

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

    public Drop(int dropWeight, Card cardTemplate, int dropNum)
    {
        dropConfig.Add(new(cardTemplate, dropNum));
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

    public Drop(int dropWeight, OutStringAction onDrop)
    {
        this.dropWeight = dropWeight;
        this.onDrop = onDrop;
    }

    public List<Card> GetDroppedCards(out string tip)
    {
        tip = string.Empty;

        var droppedCards = new List<Card>();
        foreach (var config in dropConfig)
        {
            droppedCards.AddRange(config.GetDroppedCards());
        }

        onDrop?.Invoke(out tip);

        return droppedCards;
    }

    public bool ContainsCard(string cardId)
    {
        return dropConfig.Any(d => d.ContainsCard(cardId));
    }
}