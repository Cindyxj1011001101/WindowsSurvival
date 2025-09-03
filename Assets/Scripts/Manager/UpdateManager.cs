using UnityEngine;
using UnityEngine.Events;

public class UpdateManager : MonoBehaviour
{
    private static UpdateManager instance;
    public static UpdateManager Instance => instance;

    public UnityEvent PlayerUpdate { get; private set; } = new();

    public UnityEvent EnvironmentUpdate { get; private set; } = new();

    public UnityEvent CardUpdate { get; private set; } = new();

    public UnityEvent PopulationUpdate { get; private set; } = new();

    public UnityEvent TechnologyUpdate { get; private set; } = new();

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
        CardUpdate?.Invoke();
        EnvironmentUpdate?.Invoke();
        PlayerUpdate?.Invoke();
        PopulationUpdate?.Invoke();
        TechnologyUpdate?.Invoke();
    }

    private void Clear()
    {
        PlayerUpdate?.RemoveAllListeners();
        EnvironmentUpdate?.RemoveAllListeners();
        CardUpdate?.RemoveAllListeners();
        PopulationUpdate?.RemoveAllListeners();
        TechnologyUpdate?.RemoveAllListeners();
    }
}