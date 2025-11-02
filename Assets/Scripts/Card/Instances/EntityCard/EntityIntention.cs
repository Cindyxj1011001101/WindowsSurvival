using Newtonsoft.Json;
using System;
using UnityEngine.Events;

/// <summary>
/// 实体意图
/// </summary>
public class EntityIntention
{
    [JsonProperty] private int executionCountdown;  // 意图执行倒计时
    [JsonProperty] private int preparationMinutes;  // 意图执行准备时间
    [JsonIgnore] public UnityAction action;         // 意图执行逻辑

    public EntityIntention(int preparationMinutes)
    {
        this.preparationMinutes = preparationMinutes;
    }

    public void Prepare() => executionCountdown = preparationMinutes;

    /// <summary>
    /// 更新意图执行倒计时，返回true时代表准备结束
    /// </summary>
    /// <returns></returns>
    public bool UpdatePreparationCountdown()
    {
        executionCountdown--;
        if (executionCountdown <= 0)
        {
            executionCountdown = 0;
            Execute();

            return true;
        }

        return false;
    }

    private void Execute() => action?.Invoke();
}