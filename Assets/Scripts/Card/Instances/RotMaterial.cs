using System.Collections.Generic;

/// <summary>
/// 腐烂物
/// </summary>
public class RotMaterial : Card
{
    private RotMaterial()
    {
        Events = new()
        {
            new Event("食用", "吃这个？你疯了",Event_Eat, null, () => 15,
            () => new Dictionary<PlayerStateEnum, float>() { { PlayerStateEnum.Fullness, 6 }, { PlayerStateEnum.San, -20 }, { PlayerStateEnum.Health, -10 } })
        };
    }

    private void Event_Eat(out string tip)
    {
        DestroyThis();

        tip = string.Empty;
        // 播放吃的音效
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("吃_01", true);
        //+6饱食
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 6);
        //-20精神值
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.San, -20);
        //-10健康
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, -10);
        //消耗15分钟
        TimeManager.Instance.AddTime(15);
    }
}