/// <summary>
/// 腌渍中的海蜇皮
/// </summary>
public class PickledJellyfishSkin : Card
{
    private TimerComponent timer;

    private PickledJellyfishSkin()
    {
        Events = new()
        {
            new CardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Fullness, 15 },
                { PlayerStateEnum.Health, -4 },
                { PlayerStateEnum.San, -5 },
                { PlayerStateEnum.Itchiness, +45 }
            }),
        };
    }

    public override void Awake()
    {
        base.Awake();
        if (!TryGetComponent(out timer))
        {
            timer = new(720)
            {
                tipText = "腌渍完成"
            };
            AddComponent(timer);
        }
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        HandlePickle();
    }

    private void HandlePickle()
    {
        timer.AddValue(-TimeManager.Instance.SettleInterval);

        if (timer.value <= 0)
        {
            // 腌渍完成，变为“已处理的海蜇皮”
            TurnTo("已处理的海蜇皮", Bag);
        }
    }
}
