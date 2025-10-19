/// <summary>
/// 灵光乍现
/// </summary>
public class FlashOfInspiration : GameEvent
{
    private const float SAN_THRESHOLD = 0.85f; // 精神状态阈值

    public override bool CanTriggerThisEvent()
    {
        var san = StateManager.Instance.PlayerStateDict[PlayerStateEnum.San];
        return san.CurValue / san.MaxValue >= SAN_THRESHOLD;
    }

    protected override void OnTrigger()
    {
        // 立刻完成当前科技
        if (TechnologyManager.Instance.CurStudiedTechNode != null)
        {
            TechnologyManager.Instance.AddStudyProcess(9999); // 研究进度增加
        }
    }
}
