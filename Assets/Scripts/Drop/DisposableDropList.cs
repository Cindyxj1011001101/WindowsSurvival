using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DisposableDropList
{
    public int maxCount;

    public List<Drop> dropList = new List<Drop>();

    [JsonIgnore]
    public bool IsEmpty => dropList.IsNullOrEmpty();

    /// <summary>
    /// 剩余掉落占比
    /// </summary>
    [JsonIgnore]
    public float RemainingDropsRate => (float)dropList.Count / maxCount;

    /// <summary>
    /// 随机掉落
    /// </summary>
    /// <returns></returns>
    public List<Card> RandomDrop()
    {
        if (dropList.Count == 0)
        {
            return new();
        }

        // 计算总概率
        int totalProb = 0;
        foreach (var drop in dropList)
        {
            totalProb += drop.dropProb;
        }

        // 随机选择
        int randomValue = Random.Range(0, totalProb);
        int currentProb = 0;

        for (int i = 0; i < dropList.Count; i++)
        {
            currentProb += dropList[i].dropProb;
            if (randomValue < currentProb)
            {
                // 获取掉落项
                Drop drop = dropList[i];

                // 从剩余列表中移除（一次性掉落）
                dropList.RemoveAt(i);

                return drop.GetDroppedCards(out _);
            }
        }

        // 理论上不会执行到这里
        Debug.LogError("Drop selection failed!");
        return null;
    }

    /// <summary>
    /// 掉落指定卡牌
    /// </summary>
    /// <param name="cardId"></param>
    /// <returns></returns>
    public List<Card> CertainDrop(string cardId)
    {
        for (int i = 0; i < dropList.Count; i++)
        {
            Drop drop = dropList[i];
            if (drop.card.CardId == cardId)
            {
                dropList.RemoveAt(i);
                return drop.GetDroppedCards(out _);
            }
        }

        return null;
    }
}