/// <summary>
/// 止痛药
/// </summary>
[CardId("止痛药")]
public class Painkillers : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("使用", "使用止痛药。这可以缓解疼痛，但是一天内使用多次效果会变差", Event_Use, null,
            () => 5,
            () => new()
            {
                { PlayerStateEnum.PainLevel, -50 * GlobalDataManager.Instance.GlobalData.GetReduceRate(CardId) }
            },
            sound: "吃_01");
    }

    protected override void OnInit()
    {
        GlobalDataManager.Instance.GlobalData.AddReduceAction(CardId, new Reduce(2, .5f));
        EventManager.Instance.AddListener(EventType.AnotherDay, RefreshSlot);
    }

    protected override void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EventType.AnotherDay, RefreshSlot);
    }

    private void Event_Use(CardEvent e)
    {
        DestroyThis();
        ApplyEventEffects(e);
        GlobalDataManager.Instance.GlobalData.AddReduceCount(CardId);
    }
}