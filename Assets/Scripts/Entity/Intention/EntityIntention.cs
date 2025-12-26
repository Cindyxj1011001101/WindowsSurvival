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
    [JsonIgnore] public bool ExeSucceed { get; private set; }
    [JsonIgnore] public bool IsValid => !belongedEntity.Destroyed; // 当所属实体不存在时，意图失效

    protected virtual bool WithoutAnim => true;

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

    public void UpdateExecutionCountdown()
    {
        executionCountdown = Mathf.Max(executionCountdown - 1, 0);
    }

    /// <summary>
    /// 动效结束后调用
    /// </summary>
    protected void ExecuteOver()
    {
        // 刷新实体意图
        belongedEntity.RefreshIntention(); // 意图切换动画在此处，切换完毕后会调用 TimeManager.Instance.DequeueIntention
    }

    public abstract string GiveName();
    protected abstract bool CanExecute();
    public abstract void OnExecute();
    public abstract string GetDescription();
    public void TryExecute()
    {
        ExeSucceed = CanExecute();
        if (!ExeSucceed)
        {
            // 意图执行失败，立即结束
            ExecuteOver();
            return;
        }

        OnExecute();

        if (WithoutAnim)
        {
            // 对于有动效的意图，应当在动效完全结束以后调用 ExecuteOver
            ExecuteOver();
        }
    }
}