using System.Collections.Generic;

public class GlobalDataManager : IManager
{
    public static GlobalDataManager Instance { get; } = new();

    public GlobalData GlobalData { get; private set; }

    #region 卡牌数量
    // 不需要持久化，这个是运行时数据
    // 不需要持久化，这个是运行时数据
    private Dictionary<string, int> cardNumDict = new(); // 卡牌数量

    public void AddCardNum(string cardId, int num = 1)
    {
        if (cardNumDict.ContainsKey(cardId))
        {
            cardNumDict[cardId] += num;
        }
        else
        {
            cardNumDict.Add(cardId, num);
        }

        EventManager.Instance.TriggerEvent(EventType.CardNumChange, (cardId, cardNumDict[cardId]));
    }

    public void ReduceCardNum(string cardId, int num = 1)
    {
        if (cardNumDict.ContainsKey(cardId))
        {
            cardNumDict[cardId] -= num;

            EventManager.Instance.TriggerEvent(EventType.CardNumChange, (cardId, cardNumDict[cardId]));
        }
    }

    public int GetCardNum(string cardId)
    {
        if (cardNumDict.TryGetValue(cardId, out var num))
        {
            return num;
        }
        return 0;
    }
    #endregion

    #region 实体记录
    private Dictionary<string, IEntity> allEntities = new();

    public void AddEntity(IEntity entity)
    {
        if (allEntities.ContainsKey(entity.UUID)) return;

        allEntities.Add(entity.UUID, entity);
    }

    public void RemoveEntity(string uuid)
    {
        if (!allEntities.ContainsKey(uuid)) return;

        allEntities.Remove(uuid);
    }

    public IEntity GetEntity(string uuid)
    {
        if (allEntities.ContainsKey(uuid))
            return allEntities[uuid];

        return null;
    }

    public bool ExistsEntity(string uuid)
    {
        return allEntities.ContainsKey(uuid);
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