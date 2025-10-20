using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DropList
{
    public bool disposable = false; // 一次性掉落，掉落后从列表中移除
    public int maxCount;
    public List<Drop> dropList = new();

    [JsonIgnore] public bool IsEmpty => dropList.IsNullOrEmpty();

    /// <summary>
    /// 剩余掉落占比
    /// </summary>
    [JsonIgnore] public float RemainingDropsRate => (float)dropList.Count / maxCount;

    public DropList() { }

    public DropList(params Drop[] drops)
    {
        dropList = drops.ToList();
        maxCount = drops.Length;
    }

    public DropList(List<Drop> drops, bool disposable)
    {
        dropList = drops;
        maxCount = drops.Count;
        this.disposable = disposable;
    }

    private int CalcTotalWeight()
    {
        return dropList.Sum(d => d.dropWeight);
    }

    /// <summary>
    /// 随机掉落
    /// </summary>
    /// <returns></returns>
    public List<Card> RandomDrop(out string tip)
    {
        tip = string.Empty;
        if (dropList.IsNullOrEmpty())
            return new();

        // 计算总权重
        int totalWeight = CalcTotalWeight();

        // 随机选择
        int randomValue = Random.Range(0, totalWeight);
        int currentSum = 0;

        for (int i = 0; i < dropList.Count; i++)
        {
            currentSum += dropList[i].dropWeight;
            if (randomValue < currentSum)
            {
                // 获取掉落项
                Drop drop = dropList[i];

                if (disposable)
                    // 从剩余列表中移除（一次性掉落）
                    dropList.RemoveAt(i);

                return drop.GetDroppedCards(out tip);
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
            foreach (var c in drop.droppedCards)
            {
                if (c.CardId == cardId)
                {
                    if (disposable)
                        // 从剩余列表中移除（一次性掉落）
                        dropList.RemoveAt(i);

                    return drop.GetDroppedCards(out _);
                }
            }
        }

        return null;
    }
}