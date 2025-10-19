using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InvasionEventConfig
{
    public float basicIntensity = 1f;               // 基础入侵事件强度
    public float maxSurvivalDayEffect = 50f;        // 生存天数最大影响上限
    public float threatCoefficient = 1f;            // 威胁系数
    public float minRandomFactor = 0.7f;            // 最小随机系数
    public float maxRandomFactor = 1.3f;            // 最大随机系数
}

/// <summary>
/// 入侵
/// </summary>
public class Invasion : GameEvent
{
    private List<InvasionComposition> allCompositions = new();

    public override void OnTrigger()
    {
        RegisterAllCompositions();
        GenerateInvasion(CalculateThreatIntensity());
    }

    private void RegisterAllCompositions()
    {
        if (allCompositions.IsNullOrEmpty())
            allCompositions = ExcelReader.ReadInvasionCompositionConfig("InvasionCompositionConfig");
    }

    /// <summary>
    /// 生成入侵事件
    /// </summary>
    /// <param name="threatIntensity">威胁强度</param>
    /// <returns>入侵结果</returns>
    public void GenerateInvasion(float threatIntensity)
    {
        // 第一步：判断满足条件的组合，并抽取一个组合
        var selectedComposition = SelectInvasionComposition(threatIntensity);
        if (selectedComposition == null)
        {
            Debug.LogError("没有找到合适的入侵组合，入侵事件生成失败。");
            return;
        }

        var creatures = selectedComposition.Generate(threatIntensity);
        Debug.Log($"入侵事件生成成功，威胁强度：{threatIntensity}，入侵组合：{selectedComposition.compositionName}，入侵生物数量：{creatures.Count}");
        foreach (var creature in creatures)
        {
            Debug.Log($"入侵生物：{creature}");
        }

        // 将入侵生物添加到游戏中
        var curEnv = GameManager.Instance.CurEnvironmentBag;
        EnvironmentBag spawnEnv = curEnv.PlaceData.isIndoor ? GameManager.Instance.EnvironmentBags[curEnv.PlaceData.connectedOutdoorPlace] : curEnv; // 入侵生物的生成地点
        GameManager.Instance.AddCardsToTargetEnv(creatures, spawnEnv);
    }

    /// <summary>
    /// 选择入侵组合
    /// </summary>
    private InvasionComposition SelectInvasionComposition(float threatIntensity)
    {
        // 筛选满足条件的组合
        var validCompositions = allCompositions
            .Where(c => c.CanGenerate(threatIntensity))
            .ToList();

        if (validCompositions.IsNullOrEmpty()) return null;

        // 根据权重随机选择
        float totalWeight = validCompositions.Sum(c => c.compositionWeight);
        float randomValue = Random.value * totalWeight;
        float currentSum = 0f;

        foreach (var composition in validCompositions)
        {
            currentSum += composition.compositionWeight;
            if (randomValue <= currentSum)
                return composition;
        }

        return validCompositions.Last();
    }
}
