using System.Collections.Generic;
using UnityEngine;

public class GlobalDataManager : MonoBehaviour
{
    private static GlobalDataManager instance;
    public static GlobalDataManager Instance => instance;

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

    private void Awake()
    {
        instance = this;
        GlobalData = GameDataManager.Instance.GlobalData;
        EventManager.Instance.AddListener(EventType.AnotherDay, OnAnotherDay);
    }

    private void OnDestroy()
    {
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