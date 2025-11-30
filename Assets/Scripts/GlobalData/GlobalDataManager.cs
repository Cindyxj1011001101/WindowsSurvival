using System.Collections.Generic;

public class GlobalDataManager : IManager
{
    public static GlobalDataManager Instance { get; } = new();

    public GlobalData GlobalData { get; private set; }

    #region 卡牌数量
    // 不需要持久化，这个是运行时数据
    // 不需要持久化，这个是运行时数据
    private Dictionary<string, int> cardNumDict = new(); // 卡牌数量

    private void IncreaseCardNum(string cardId)
    {
        if (cardNumDict.ContainsKey(cardId))
            cardNumDict[cardId]++;
        else
            cardNumDict.Add(cardId, 1);

        EventManager.Instance.TriggerEvent(EventType.CardNumChange, (cardId, cardNumDict[cardId]));
    }

    private void DecreaseCardNum(string cardId)
    {
        if (!cardNumDict.ContainsKey(cardId)) return;

        cardNumDict[cardId]--;
        EventManager.Instance.TriggerEvent(EventType.CardNumChange, (cardId, cardNumDict[cardId]));
    }

    public int GetCardNum(string cardId)
    {
        if (cardNumDict.TryGetValue(cardId, out var num))
            return num;

        return 0;
    }
    #endregion

    #region 实体记录
    private Dictionary<string, IEntity> allEntities = new();

    public void CreateEntity(IEntity entity)
    {
        if (allEntities.ContainsKey(entity.Uuid)) return;

        allEntities.Add(entity.Uuid, entity);
    }

    public void DestroyEntity(IEntity entity)
    {
        if (!allEntities.ContainsKey(entity.Uuid)) return;

        allEntities.Remove(entity.Uuid);
    }

    public IEntity GetEntityByUuid(string uuid)
    {
        if (allEntities.ContainsKey(uuid))
            return allEntities[uuid];

        return null;
    }
    #endregion

    #region 卡牌记录
    private Dictionary<string, Card> allCards = new();

    public void CreateCard(Card card)
    {
        if (allCards.ContainsKey(card.Uuid)) return;

        allCards.Add(card.Uuid, card);
        IncreaseCardNum(card.CardId);
    }

    public void DestroyCard(Card card)
    {
        if (!allCards.ContainsKey(card.Uuid)) return;

        allCards.Remove(card.Uuid);
        DecreaseCardNum(card.CardId);
    }

    public Card GetCardByUuid(string uuid)
    {
        if (allCards.ContainsKey(uuid))
            return allCards[uuid];

        return null;
    }
    #endregion

    public void Init()
    {
        GlobalData = GameDataManager.Instance.GlobalData;
        EventManager.Instance.AddListener(EventType.AnotherDay, OnAnotherDay);
    }

    public void Reset()
    {
        cardNumDict.Clear();
        allEntities.Clear();
        allCards.Clear();
        GlobalData = null;
        EventManager.Instance.RemoveListener(EventType.AnotherDay, OnAnotherDay);
    }

    private void OnAnotherDay()
    {
        ResetReduceCount();
    }

    private void ResetReduceCount()
    {
        foreach (var reduce in GlobalData.reduceActionDict.Values)
        {
            reduce.curReduceCount = 0;
        }
    }
}