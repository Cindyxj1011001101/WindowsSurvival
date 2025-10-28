public class SunlightManager : IManager
{
    public static SunlightManager Instance { get; } = new SunlightManager();

    public float Sunlight { get; private set; } = 20f; // 恒星光照值

    public void Init()
    {
        // TODO: 读取存档的光照值

        UpdateManager.Instance.SunlightUpdate.AddListener(Update);
        EventManager.Instance.AddListener<GameEvent>(EventType.OnGameEventTrigger, OnStellarEclipseTrigger);
    }

    public void Reset()
    {
        UpdateManager.Instance.SunlightUpdate.RemoveListener(Update);
        EventManager.Instance.RemoveListener<GameEvent>(EventType.OnGameEventTrigger, OnStellarEclipseTrigger);
    }

    private void OnStellarEclipseTrigger(GameEvent gameEvent)
    {
        if (gameEvent.GetType() != typeof(StellarEclipse)) return;

        SetSunlight(0f);
    }

    private void SetSunlight(float sunlight)
    {
        if (sunlight == Sunlight) return;

        Sunlight = sunlight;
        EventManager.Instance.TriggerEvent(EventType.UpdateSunlight, Sunlight);
    }

    private void Update()
    {
        // TODO: 处理一天内的光照变化


        // 恒星食期间光照始终为0
        if (GameEventManager.Instance.IsEventOngoing<StellarEclipse>())
        {
            SetSunlight(0f);
            return;
        }
    }
}