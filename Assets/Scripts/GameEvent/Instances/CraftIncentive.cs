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

    public override bool CanTriggerThisEvent()
    {
        // TODO: 需要不在睡觉中
        var san = StateManager.Instance.PlayerStateDict[PlayerStateEnum.Sanity];
        return san.CurValue / san.MaxValue >= SAN_THRESHOLD;
    }

    public override void OnTrigger()
    {
        
    }
}
