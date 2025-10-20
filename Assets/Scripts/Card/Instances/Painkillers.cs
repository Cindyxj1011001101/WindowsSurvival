/// <summary>
/// 止痛药
/// </summary>
public class Painkillers : Card
{
    private Painkillers()
    {
        Events = new()
        {
            new CardEvent("使用", "使用止痛药。这可以缓解疼痛，但是一天内使用多次效果会变差", Event_Use, null, () => 5,
            () => new ()
            {
                { PlayerStateEnum.PainLevel, -50 * GlobalDataManager.Instance.GlobalData.GetReduceRate(CardId) }
            })
        };
    }

    public override void Init()
    {
        base.Init();
        GlobalDataManager.Instance.GlobalData.AddReduceAction(CardId, new Reduce(2));

        EventManager.Instance.AddListener(EventType.AnotherDay, RefreshSlot);
    }

    protected override void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EventType.AnotherDay, RefreshSlot);
    }

    private void Event_Use(out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        // 播放吃的音效
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("吃_01", true);

        StateManager.Instance.ChangePlayerState(PlayerStateEnum.PainLevel, -50 * GlobalDataManager.Instance.GlobalData.GetReduceRate(CardId));

        GlobalDataManager.Instance.GlobalData.AddReduceCount(CardId);

        TimeManager.Instance.AddTime(5);
    }
}