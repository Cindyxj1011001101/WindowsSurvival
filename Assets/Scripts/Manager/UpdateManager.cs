using UnityEngine;
using UnityEngine.Events;

public class UpdateManager : MonoBehaviour
{
    private static UpdateManager instance;
    public static UpdateManager Instance => instance;

    public UnityEvent InGameEventUpdate { get; private set; } = new();
    public UnityEvent PlayerUpdate { get; private set; } = new();
    public UnityEvent EnvironmentUpdate { get; private set; } = new();
    public UnityEvent CardUpdate { get; private set; } = new();
    public UnityEvent PopulationUpdate { get; private set; } = new();
    public UnityEvent TechnologyUpdate { get; private set; } = new();
    public UnityEvent SunlightUpdate { get; private set; } = new();

    private void Awake()
    {
        instance = this;
        EventManager.Instance.AddListener(EventType.Update, OnUpdate);
    }

    private void OnDestroy()
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
        InGameEventUpdate.Invoke();
        SunlightUpdate.Invoke();
    }

    private void Clear()
    {
        InGameEventUpdate.RemoveAllListeners();
        PlayerUpdate.RemoveAllListeners();
        EnvironmentUpdate.RemoveAllListeners();
        CardUpdate.RemoveAllListeners();
        PopulationUpdate.RemoveAllListeners();
        TechnologyUpdate.RemoveAllListeners();
        SunlightUpdate.RemoveAllListeners();
    }
}