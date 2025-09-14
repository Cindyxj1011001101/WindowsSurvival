using System.Collections.Generic;
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
                return dropList[i].GetDroppedCards(out tip);
            }
        }

        // 理论上不会执行到这里
        Debug.LogError("Drop selection failed!");
        return null;
    }
}