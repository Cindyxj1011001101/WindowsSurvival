using System.Collections.Generic;

/// <summary>
/// 压缩饼干
/// </summary>
public class CompactBiscuit : Card
{
    private CompactBiscuit()
    {
        Events = new()
        {
            new Event("食用", "食用压缩饼干", Event_Eat, null, null, 3,
            new Dictionary<PlayerStateEnum, float>() { { PlayerStateEnum.Fullness, 12 } })
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