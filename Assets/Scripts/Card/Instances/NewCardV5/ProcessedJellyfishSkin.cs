/// <summary>
/// 已处理的海蜇皮
/// </summary>
public class ProcessedJellyfishSkin : Card
{
    private ProcessedJellyfishSkin()
    {
        Events = new()
        {
            new Event("食用", "", Event_Eat, null, () => 15,
            () => new() { { PlayerStateEnum.Fullness, 25 }, { PlayerStateEnum.Itchiness, +5 } }),
        };
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
}
