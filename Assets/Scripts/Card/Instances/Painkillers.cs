using UnityEngine;

/// <summary>
/// 止痛药
/// </summary>
public class Painkillers : Card
{
    public int maxReduceCount = 2;
    public int curReduceCount = 0;
    public float reduceRate = 0.5f;
    private Painkillers()
    {
        Events = new()
        {
            new Event("使用", "一天内多次使用效果减半,最多叠加2次", Event_Use, null, () => 5,  () => new () { { PlayerStateEnum.PainLevel, -50 * Mathf.Pow(reduceRate, curReduceCount) } })
        };
    }
    private void Event_Use(out string tip)
    {
        DestroyThis();
        tip = string.Empty;
        // 播放吃的音效
        if(SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("吃_01",true);
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.PainLevel, -50 * Mathf.Pow(reduceRate, curReduceCount));
        TimeManager.Instance.AddTime(5);
        curReduceCount++;
        if (curReduceCount >= maxReduceCount) curReduceCount = maxReduceCount;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (TimeManager.Instance.AnotherDay())
        {
            curReduceCount = 0; // 隔天时刷新可使用次数
            RefreshSlot();
        }
    }
}