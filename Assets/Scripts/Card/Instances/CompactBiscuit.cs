/// <summary>
/// 压缩饼干
/// </summary>
public class CompactBiscuit : Card
{
    private CompactBiscuit()
    {
        events = new()
        {
            new Event("食用", "食用压缩饼干", Event_Eat, null)
        };
    }

    public void Event_Eat()
    {
        DestroyThis();
        // 播放吃的音效
        if(SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("吃_01",true);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 12);
        TimeManager.Instance.AddTime(3);
    }
}