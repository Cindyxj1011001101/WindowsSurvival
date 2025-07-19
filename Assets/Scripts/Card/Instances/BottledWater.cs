/// <summary>
/// 瓶装水
/// </summary>
public class BottledWater : Card
{
    private BottledWater()
    {
        events = new()
        {
            new Event("饮用", "饮用瓶装水", Event_Drink, null),
        };
    }

    public void Event_Drink()
    {
        DestroyThis();
        // 播放喝水的音效
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("喝_01", true);
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Thirst, 15));
        TimeManager.Instance.AddTime(3);
    }
}