using UnityEngine;

/// <summary>
/// 止痛药
/// </summary>
public class Painkillers : Card
{
    public int maxReduceCount = 2;
    public int curReduceCount = 0;
    public float ReduceRate = 0.5f;
    private Painkillers()
    {
        Events = new()
        {
            new Event("使用", "使用", Event_Use, null, () => 5,  () => new () { { PlayerStateEnum.PainLevel, -50 * Mathf.Pow(ReduceRate, curReduceCount) } })
        };
    }
    private void Event_Use(out string tip)
    {
        tip = string.Empty;
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.PainLevel, -50 * Mathf.Pow(ReduceRate, curReduceCount));
        TimeManager.Instance.AddTime(5);
        curReduceCount++;
        if (curReduceCount >= maxReduceCount) curReduceCount = maxReduceCount;

    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        if (TimeManager.Instance.AnotherDay()) curReduceCount = 0;
    }
}