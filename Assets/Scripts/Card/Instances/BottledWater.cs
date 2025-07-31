using System.Collections.Generic;

/// <summary>
/// 瓶装水
/// </summary>
public class BottledWater : Card
{
    private BottledWater()
    {
        Events = new()
        {
            new Event("饮用", "连瓶子也喝掉", Event_Drink, null,15,
            new Dictionary<PlayerStateEnum, float>() { { PlayerStateEnum.Thirst, 15 } })
        };
    }

    public void Event_Drink()
    {
        DestroyThis();
        // 播放喝水的音效
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("喝_01", true);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Thirst, 15);
        TimeManager.Instance.AddTime(3);
    }

}