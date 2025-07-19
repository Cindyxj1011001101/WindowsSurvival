using System;

/// <summary>
/// 小块肉
/// </summary>
public class LittleRawMeat : Card
{
    public LittleRawMeat()
    {
        events = new()
        {
            new Event("食用", "食用小块生肉", Event_Eat, null)
        };
    }

    private void OnRotton()
    {
        DestroyThis();
        GameManager.Instance.AddCard("腐烂物", true);
    }

    public void Event_Eat()
    {
        DestroyThis();
        // 播放吃的音效
        if(SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("吃_01",true);
        //+12饱食
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Fullness, 12));
        //-2精神值
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.San, -2));
        //-3健康
        StateManager.Instance.OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Health, -3));
        //消耗15分钟
        TimeManager.Instance.AddTime(15);
    }

    protected override Action OnUpdate => () =>
    {
        TryGetComponent<FreshnessComponent>(out var component);
        component.Update(TimeManager.Instance.SettleInterval, OnRotton);
    };
}