using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// 实体意图
/// </summary>
public abstract class EntityIntention
{
    [JsonProperty] protected int executionCountdown;    // 意图执行倒计时
    [JsonProperty] protected int preparationMinutes;    // 意图执行准备时间
    [JsonIgnore] protected EntityCard belongedEntity;   // 所属实体

    [JsonIgnore] public int ExecutionCountdown => executionCountdown;
    [JsonIgnore] public bool IsReady => executionCountdown <= 0;

    public EntityIntention(int preparationMinutes)
    {
        this.preparationMinutes = preparationMinutes;
    }

    public void SetBelongedEntity(EntityCard entity)
    {
        belongedEntity = entity;
    }

    public void Prepare()
    {
        executionCountdown = preparationMinutes;
    }

    /// <summary>
    /// 更新意图执行倒计时，返回true时代表准备结束
    /// </summary>
    /// <returns></returns>
    public void UpdateExecutionCountdown()
    {
        executionCountdown = Mathf.Max(executionCountdown - 1, 0);
    }

    public abstract string GiveName();
    public abstract bool CanExecute();
    public abstract void Execute();
    public abstract string GetDescription();
}