/// <summary>
/// 被捉住的水瓶鱼
/// </summary>
public class CaughtAquariusFishWithProduct : Card
{
    public CaughtAquariusFishWithProduct()
    {
        events = new()
        {
            new Event("饮用", "饮用水瓶鱼", Event_Drink, null),
            new Event("放生", "放生水瓶鱼", Event_Release, Judge_Release),
        };
    }

    public void Event_Drink()
    {
        DestroyThis();
        // 播放喝水的音效
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("喝_01", true);
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Thirst, 15));
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Fullness, 4));
        TimeManager.Instance.AddTime(15);
    }

    public void Event_Release()
    {
        DestroyThis();
        // 地点中增加一个有产物的水瓶鱼
        GameManager.Instance.AddCard("有产物的水瓶鱼", true);
    }

    public bool Judge_Release()
    {
        return GameManager.Instance.CurEnvironmentBag.PlaceData.isInWater;
    }
}