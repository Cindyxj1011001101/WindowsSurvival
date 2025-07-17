using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 种群
/// </summary>
[System.Serializable]
public class Population
{
    public Card card; // 卡牌
    public int dropNum; // 掉落数量

    public int curSize; // 当前数量
    public int maxSize; // 最大数量

    public int sizeChangePerRound; // 每回合数量变化
    public int sizeChangeOnCaught; // 捕捞后的数量变化

    public int Proportion => Mathf.Clamp(curSize, 0, maxSize);

    /// <summary>
    /// 每回合更新种群数量
    /// </summary>
    public void Update()
    {
        curSize += sizeChangePerRound;
        if (curSize > maxSize) curSize = maxSize;
    }

    /// <summary>
    /// 被捕捞
    /// </summary>
    public void GetCaught()
    {
        curSize += sizeChangeOnCaught;
        if (curSize > maxSize) curSize = maxSize;
    }
}

[System.Serializable]
/// <summary>
/// 重复掉落列表
/// </summary>
public class RepeatableDropList
{
    public int emptyPopulationSizeChangeOnNotCaught;
    public Population emptyPopulation = new();
    public List<Population> populationList = new(); // 种群列表

    [JsonIgnore]
    public bool IsEmpty => populationList.Count == 0;

    public void Init(List<Population> populationList)
    {
        this.populationList = populationList;
    }

    /// <summary>
    /// 种群数量开始变化
    /// </summary>
    public void StartUpdating()
    {
        if (IsEmpty) return;

        EventManager.Instance.AddListener(EventType.IntervalSettle, () =>
        {
            foreach (var population in populationList)
            {
                population.Update();
            }
            emptyPopulation.Update();
        });
    }

    /// <summary>
    /// 打捞
    /// </summary>
    public List<Card> RandomDrop()
    {
        // 计算总概率
        int totalProb = emptyPopulation.Proportion;
        foreach (var population in populationList)
        {
            totalProb += population.Proportion;
        }

        // 随机选择
        int randomValue = Random.Range(0, totalProb);
        int currentProb = 0;

        foreach (var population in populationList)
        {
            currentProb += population.Proportion;
            if (randomValue < currentProb)
            {
                // 抽中种群
                population.GetCaught();
                // 空种群数量增加
                emptyPopulation.curSize += emptyPopulationSizeChangeOnNotCaught;
                if (emptyPopulation.curSize > emptyPopulation.maxSize)
                    emptyPopulation.curSize = emptyPopulation.maxSize;
                return DropCards(population);
            }
        }

        // 抽中空种群

        return null;
    }

    private List<Card> DropCards(Population population)
    {
        List<Card> droppedCards = new List<Card>();
        // 创建卡牌
        for (int j = 0; j < population.dropNum; j++)
        {
            // 添加到返回列表
            if (population.card != null)
            {
                // 深拷贝
                droppedCards.Add(JsonManager.DeepCopy(population.card));
            }
        }
        return droppedCards;
    }
}
