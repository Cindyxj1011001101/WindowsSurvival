using UnityEngine;

public class SpaceshipSeat : ConstructionCard
{
    public int curReduceCount = 0;
    public int maxReduceCount = 2;
    public float reduceRate = 0.5f;
    private SpaceshipSeat()
    {
        Events = new()
        {
            new Event("靠着休息", "靠着休息", Event_Rest, Judge_Rest),
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
    private void Event_Rest(out string tip)
    {
        tip = string.Empty;
        //TODO:唤起时间窗口，设置休息时长为0-60分钟
        curReduceCount++;
    }
    private bool Judge_Rest(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.CurEnvironmentBag.PlaceData.isInWater)
        {
            hint = "只能休息在非水域环境";
            return false;
        }
        return true;
    }

    private void StartSleeping()
    {
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Sobriety, +2.7f * Mathf.Pow(reduceRate, curReduceCount));
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.San, +2f * Mathf.Pow(reduceRate, curReduceCount));
    }

    private void StopSleeping()
    {
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Sobriety, -2.7f * Mathf.Pow(reduceRate, curReduceCount));
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.San, -2f * Mathf.Pow(reduceRate, curReduceCount));
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (TimeManager.Instance.AnotherDay()) curReduceCount = 0; // 隔天时刷新可使用次数
    }
}