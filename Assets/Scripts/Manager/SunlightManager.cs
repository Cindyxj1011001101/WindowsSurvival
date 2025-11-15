public class SunlightManager : IManager
{
    public static SunlightManager Instance { get; } = new();

    public float Sunlight { get; private set; } = 20f; // 恒星光照值

    public void Init()
    {
        // TODO: 读取存档的光照值

        UpdateManager.Instance.SunlightUpdate.AddListener(Update);
    }

    public void Reset()
    {
        UpdateManager.Instance.SunlightUpdate.RemoveListener(Update);
    }

    private void SetSunlight(float sunlight)
    {
        if (sunlight == Sunlight) return;

        Sunlight = sunlight;
        EventManager.Instance.TriggerEvent(EventType.UpdateSunlight, Sunlight);
    }

    private void Update()
    {
        // 由于事件更新早于光照更新，所以可以不用监听StellarEclipse事件
        // 恒星食期间光照始终为0
        if (GameEventManager.Instance.IsEventOngoing<StellarEclipse>())
        {
            SetSunlight(0f);
            return;
        }

        // TODO: 处理一天内的光照变化


    }
}