/// <summary>
/// 腌渍中的海蜇皮
/// </summary>
[CardId("腌渍中的海蜇皮")]
public class PickledJellyfishSkin : Card
{
    private TimerComponent timer;

    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 15 },
                { PlayerStateEnum.Health, -4 },
                { PlayerStateEnum.Sanity, -5 },
                { PlayerStateEnum.Itchiness, +45 }
            },
            sound: "吃_01");
    }

    protected override void OnLateConstructor()
    {
        timer = new(720)
        {
            tipText = "腌渍完成"
        };
        AddComponent(timer);
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        HandlePickle();
    }

    private void HandlePickle()
    {
        timer.AddValue(-TimeManager.SETTLEMENT_INTERVAL);

        if (timer.value <= 0)
        {
            // 腌渍完成，变为“已处理的海蜇皮”
            TurnTo("已处理的海蜇皮", Bag);
        }
    }
}
