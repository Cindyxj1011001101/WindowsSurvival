using UnityEngine.Events;

public class UpdateManager : IManager
{
    public static UpdateManager Instance { get; } = new();

    public UnityEvent GameEventUpdate { get; private set; } = new();
    public UnityEvent PlayerUpdate { get; private set; } = new();
    public UnityEvent EnvironmentUpdate { get; private set; } = new();
    public UnityEvent CardUpdate { get; private set; } = new();
    public UnityEvent PopulationUpdate { get; private set; } = new();
    public UnityEvent TechnologyUpdate { get; private set; } = new();
    public UnityEvent SunlightUpdate { get; private set; } = new();

    public void Init()
    {
        EventManager.Instance.AddListener(EventType.Update, OnUpdate);
    }

    public void Reset()
    {
        Clear();
        EventManager.Instance.RemoveListener(EventType.Update, OnUpdate);
    }

    private void OnUpdate()
    {
        EventManager.Instance.TriggerEvent(EventType.UpdateBegin);
        // 顺序很重要
        TechnologyUpdate.Invoke();
        CardUpdate.Invoke();
        EnvironmentUpdate.Invoke();
        PlayerUpdate.Invoke();
        PopulationUpdate.Invoke();
        GameEventUpdate.Invoke();
        SunlightUpdate.Invoke();
    }

    private void Clear()
    {
        GameEventUpdate.RemoveAllListeners();
        PlayerUpdate.RemoveAllListeners();
        EnvironmentUpdate.RemoveAllListeners();
        CardUpdate.RemoveAllListeners();
        PopulationUpdate.RemoveAllListeners();
        TechnologyUpdate.RemoveAllListeners();
        SunlightUpdate.RemoveAllListeners();
    }
}