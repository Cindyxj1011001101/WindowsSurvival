public abstract class GlobalEffect
{
    public int Duration { get; private set; }

    public GlobalEffect(int duration)
    {
        Duration = duration;
    }

    public virtual void OnBegin()
    {
        UnityEngine.Debug.Log($"触发全局效果。效果：{GetType().Name}，持续时间：{Duration}");
        EventManager.Instance.TriggerEvent(EventType.OnGlobalEffectBegin, this);
    }

    public virtual void OnUpdate()
    {
        Duration -= TimeManager.SETTLEMENT_INTERVAL;
    }

    public virtual void OnEnd()
    {
        UnityEngine.Debug.Log($"结束全局效果。效果：{GetType().Name}");
        EventManager.Instance.TriggerEvent(EventType.OnGlobalEffectEnd, this);
    }
}