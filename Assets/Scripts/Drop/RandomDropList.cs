using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomDropList
{
    public List<Drop> dropList = new();

    public RandomDropList() { }

    public RandomDropList(params Drop[] drops)
    {
        dropList = new(drops);
    }

    public List<Card> RandomDrop(out string tip)
    {
        tip = string.Empty;
        if (dropList.IsNullOrEmpty())
        {
            return new();
        }

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
                return dropList[i].GetDroppedCards(out tip);
            }
        }

        // 理论上不会执行到这里
        Debug.LogError("Drop selection failed!");
        return null;
    }

    private int CalcTotalWeight()
    {
        return dropList.Sum(d => d.dropWeight);
    }
}