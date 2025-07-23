/// <summary>
/// 腐烂物
/// </summary>
public class RotMaterial : Card
{
    private RotMaterial()
    {
        Events = new()
        {
            new Event("食用", "食用腐烂物", Event_Eat, null)
        };
    }

    public void Event_Eat()
    {
        DestroyThis();
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