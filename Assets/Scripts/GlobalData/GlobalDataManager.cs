using System.Collections.Generic;
using UnityEngine;

public class GlobalDataManager : MonoBehaviour
{
    private static GlobalDataManager instance;
    public static GlobalDataManager Instance => instance;

    public GlobalData globalData;
    public GlobalData saveData;

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

    public void RemoveCardNum(string cardId, int num = 1)
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

        globalData = GameDataManager.Instance.GlobalData;
        saveData = GameDataManager.Instance.SaveData;

        EventManager.Instance.AddListener(EventType.AnotherDay, OnAnotherDay);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EventType.AnotherDay, OnAnotherDay);
    }

    private void OnAnotherDay()
    {
        SolveReduce();
    }

    private void SolveReduce()
    {
        foreach (var reduce in saveData.reduceActionDict.Values)
        {
            reduce.curReduceCount = 0;
        }
    }
}