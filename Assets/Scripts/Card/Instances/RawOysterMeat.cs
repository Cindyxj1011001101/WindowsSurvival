/// <summary>
/// 生贝肉
/// </summary>
public class RawOysterMeat : Card
{
    private RawOysterMeat()
    {
        events = new()
        {
            new Event("食用", "食用生贝肉", Event_Eat, null),
        };
    }

    private void OnRotton()
    {
        DestroyThis();
    }

    #region 食用
    public void Event_Eat()
    {
        DestroyThis();
        // 播放吃的音效
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("吃_01", true);
        //+6饱食
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Fullness, 6);
        //-1.2健康
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, -1.2f);
        //消耗5分钟
        TimeManager.Instance.AddTime(5);
    }
    #endregion

    protected override System.Action OnUpdate => () =>
    {
        TryGetComponent<FreshnessComponent>(out var component);
        component.Update(TimeManager.Instance.SettleInterval, OnRotton);
    };
}