using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

/// <summary>
/// 入侵组合
/// </summary>
public abstract class InvasionComposition
{
    private static Dictionary<string, float> creatureThreatPointsDict = new()
    {
        { "吸盘蠕虫", 2f },
        { "裙水母", 1.2f },
        { "狮子水母", 3.5f },
    };

    private static Dictionary<string, Type> compositiontNameTypeDict = new()
    {
        { "蠕虫入侵", typeof(WormInvasion) },
        { "水母入侵", typeof(JellyfishInvasion) },
    };

    public string compositionName;                            // 组合名称
    public (float, float) threatIntensityRange;               // 威胁强度范围
    public float compositionWeight;                           // 权重
    public Dictionary<string, float> invasionCreatureWeights; // 入侵生物权重

    public static InvasionComposition ParseDataRow(DataRow row)
    {
        var compositionName = row[0].ToString();
        if (string.IsNullOrEmpty(compositionName)) return null; // 如果组合名称为空，跳过读取

        if (!compositiontNameTypeDict.TryGetValue(compositionName, out Type compositionType))
        {
            Debug.LogError($"未知的入侵组合名称: {compositionName}");
            return null;
        }

        InvasionComposition instance = (InvasionComposition)Activator.CreateInstance(compositionType);
        instance.compositionName = compositionName;
        instance.threatIntensityRange = ParseThreatIntensityRange(row[1].ToString());
        instance.compositionWeight = ExcelReader.ParseFloat(row[3].ToString());
        instance.invasionCreatureWeights = ParseInvasionCreatureWeights(row);

        return instance;
    }

    private static (float, float) ParseThreatIntensityRange(string rangeStr)
    {
        var parts = rangeStr.Split('，').Select(s => s.Trim(new char[] { '[', ']', ' ' })).ToArray();

        var leftLimitStr = parts[0];
        var rightLimitStr = parts[1];

        var leftLimit = ExcelReader.ParseInt(leftLimitStr);
        int rightLimit;
        if (rightLimitStr == "Max")
            rightLimit = int.MaxValue;
        else
            rightLimit = ExcelReader.ParseInt(rightLimitStr);

        return (leftLimit, rightLimit);
    }

    private static Dictionary<string, float> ParseInvasionCreatureWeights(DataRow row)
    {
        var result = new Dictionary<string, float>();

        int colIndex = 4;

        string creatureId;
        float creatureWeight;
        while (colIndex < row.Table.Columns.Count)
        {
            creatureId = row[colIndex].ToString();

            if (string.IsNullOrEmpty(creatureId)) break; // 如果生物ID为空，跳过读取

            creatureWeight = ExcelReader.ParseFloat(row[colIndex + 1].ToString());

            result.Add(creatureId, creatureWeight);

            colIndex += 2;
        }

        return result;
    }

    private bool IsThreatIntensityInRange(float threatIntensity)
    {
        return threatIntensity >= threatIntensityRange.Item1 && threatIntensity <= threatIntensityRange.Item2;
    }

    protected virtual bool AreOtherConditionsMet(float threatIntensity)
    {
        return true;
    }

    public bool CanGenerate(float threatIntensity)
    {
        return IsThreatIntensityInRange(threatIntensity) && AreOtherConditionsMet(threatIntensity);
    }

    /// <summary>
    /// 生成入侵生物
    /// </summary>
    /// <param name="threatIntensity"></param>
    /// <returns></returns>
    public List<Card> Generate(float threatIntensity)
    {
        var result = new List<Card>();

        // 第二步：存一下威胁事件强度
        float remainingThreatPoints = threatIntensity;

        // 第三步：存一下组合里最便宜的生物的点数
        float minThreatPoints = invasionCreatureWeights.Keys.ToList().Min(c => creatureThreatPointsDict[c]);

        // 第四步：筛选出可抽取的入侵生物（初始为组合中所有生物）
        var availableCreatures = new List<string>(invasionCreatureWeights.Keys);

        // 第五步：重复抽取生物
        while (remainingThreatPoints >= minThreatPoints && !availableCreatures.IsNullOrEmpty())
        {
            // 加权随机抽取生物
            var selectedCreature = SelectCreatureByWeight(availableCreatures);
            if (selectedCreature == null) break;

            var cost = creatureThreatPointsDict[selectedCreature];
            // 检查是否能够支付该生物的威胁点数
            if (cost <= remainingThreatPoints)
            {
                // 添加生物到结果中
                result.Add(CardFactory.CreateCard(selectedCreature));
                remainingThreatPoints -= cost;
            }
            else
            {
                // 如果无法支付，将该生物从池子中移除
                availableCreatures.Remove(selectedCreature);
            }
        }

        return result;
    }

    /// <summary>
    /// 加权随机选择生物
    /// </summary>
    private string SelectCreatureByWeight(List<string> creatures)
    {
        if (creatures.IsNullOrEmpty()) return null;

        float totalWeight = creatures.Sum(c => invasionCreatureWeights[c]);
        float randomValue = UnityEngine.Random.value * totalWeight;
        float currentSum = 0f;

        foreach (var c in creatures)
        {
            currentSum += invasionCreatureWeights[c];
            if (randomValue <= currentSum)
                return c;
        }

        return creatures.Last();
    }
}