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
            new Event("食用", "", Event_Eat, null, () => 15,
            () => new() { { PlayerStateEnum.Fullness, 15 }, { PlayerStateEnum.Health, -4 }, { PlayerStateEnum.San, -5 }, { PlayerStateEnum.Itchiness, +45 } }),
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

    private void Event_Eat(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        // 播放吃的音效
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("吃_01", true);
        StateManager.Instance.ApplyPlayerStateChange(Events[0].GetPlayerEffects());
        TimeManager.Instance.AddTime(Events[0].GetTimeEffect());
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
