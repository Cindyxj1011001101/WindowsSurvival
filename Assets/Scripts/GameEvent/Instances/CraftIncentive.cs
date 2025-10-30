using UnityEngine;

/// <summary>
/// 制作激励
/// </summary>
public class CraftIncentive : GameEvent
{
    private const float SAN_THRESHOLD = 0.85f; // 精神状态阈值

    public override string GetDetails()
    {
        return $"麦麦最近精神很好，连手都变得灵巧了起来。\n\n" +
               $"在接下来的一段时间里，麦麦能{ColorManager.Colorize("-50%", ColorManager.Green)}的制作时长，" +
               $"并且制作时只消耗{ColorManager.Colorize("一半", ColorManager.Green)}材料。";
    }

    protected override bool CanTriggerThisEvent()
    {
        var san = StateManager.Instance.PlayerStateDict[PlayerStateEnum.Sanity];
        // 不在睡眠中并且精神值高于阈值
        return !StateManager.Instance.IsResting && san.CurValue / san.MaxValue >= SAN_THRESHOLD;
    }

    protected override void OnTrigger()
    {
        SetRemainingMinutes(Random.Range(60, 541));
    }
}
