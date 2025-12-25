using Newtonsoft.Json;
using System.Collections;
using UnityEngine;

/// <summary>
/// 实体意图
/// </summary>
public abstract class EntityIntention
{
    [JsonProperty] protected int executionCountdown;    // 意图执行倒计时
    [JsonProperty] protected int preparationMinutes;    // 意图执行准备时间
    [JsonIgnore] protected EntityCard belongedEntity;   // 所属实体
    [JsonIgnore] private bool isExecuting; // 意图是否在执行中

    [JsonIgnore] public int ExecutionCountdown => executionCountdown;
    [JsonIgnore] public bool IsReady => executionCountdown <= 0;
    [JsonIgnore] public bool ExeSucceed { get; private set; } = false;
    [JsonIgnore] public bool IsValid => !belongedEntity.Destroyed; // 当所属实体不存在时，意图失效

    protected virtual bool AutoExecuteOver => true;

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

    public void ExecuteOver()
    {
        isExecuting = false;
    }

    public abstract string GiveName();
    protected abstract bool CanExecute();
    public abstract void OnExecute();
    public abstract string GetDescription();
    public void TryExecute()
    {
        PublicMono.Instance.StartCoroutine(TryExecuteCo());
    }
    private IEnumerator TryExecuteCo()
    {
        isExecuting = true;
        if (CanExecute())
        {
            ExeSucceed = true;
            OnExecute();

            if (AutoExecuteOver)
            {
                // 对于有动效的意图，应当在动效完全结束以后调用 ExecuteOver
                ExecuteOver();
            }
        }
        else
        {
            // 无法执行时必须立即结束，否则会卡住等待
            ExecuteOver();
        }

        // 等待意图执行完，动效播完
        while (isExecuting)
        {
            yield return null;
        }

        // 刷新实体意图
        belongedEntity.RefreshIntention();
    }
}