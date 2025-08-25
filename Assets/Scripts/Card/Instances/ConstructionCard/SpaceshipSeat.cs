using UnityEngine;

public class SpaceshipSeat : ConstructionCard
{
    public int curReduceCount = -1;
    public int MaxReduceCount = 2;
    public float reduceRate = 0.5f;
    private SpaceshipSeat()
    {
        Events = new()
        {
            new Event("靠着休息", "靠着休息",Event_Rest,Judge_Rest),
        };
    }
    public override void LateInit()
    {
        base.LateInit();
        EventManager.Instance.AddListener(EventType.StartSleeping, StartSleeping);
        EventManager.Instance.AddListener(EventType.StopSleeping, StopSleeping);
    }

    public override void DestroyThis()
    {
        base.DestroyThis();
        EventManager.Instance.RemoveListener(EventType.StartSleeping, StartSleeping);
        EventManager.Instance.RemoveListener(EventType.StopSleeping, StopSleeping);
    }
    public void Event_Rest(out string tip)
    {
        tip = string.Empty;
        //TODO:唤起时间窗口，设置休息时长为0-60分钟
        curReduceCount++;
    }
    public bool Judge_Rest(out string hint)
    { 
        hint = string.Empty;
        if (GameManager.Instance.CurEnvironmentBag.PlaceData.isInWater)
        {
            hint = "只能休息在非水域环境";
            return false;
        }
        return true;
    }
    public void StartSleeping()
    {
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Sobriety, +2.7f*Mathf.Pow(reduceRate, curReduceCount));
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.San, +2f*Mathf.Pow(reduceRate, curReduceCount));
    }

    public void StopSleeping()
    {
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Sobriety, -2.7f*Mathf.Pow(reduceRate, curReduceCount));
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.San, -2f*Mathf.Pow(reduceRate, curReduceCount));
    }
    protected override System.Action OnUpdate => () =>
    {
        if (TimeManager.Instance.AnotherDay()) curReduceCount = -1; // 隔天时刷新可使用次数
    };
}