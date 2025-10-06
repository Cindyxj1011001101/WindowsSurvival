/// <summary>
/// 恶臭肉
/// </summary>
public class FoulSmellingMeat : CookableCard
{
    private FoulSmellingMeat()
    {
        Events = new()
        {
            new Event("食用", "", Event_Eat, null, () => 15,
            () => new() { { PlayerStateEnum.Fullness, 14 }, { PlayerStateEnum.San, -20 }, { PlayerStateEnum.Health, -15 } }),
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
