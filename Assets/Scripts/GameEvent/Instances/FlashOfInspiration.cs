/// <summary>
/// 灵光乍现
/// </summary>
public class FlashOfInspiration : GameEvent
{
    private const float SAN_THRESHOLD = 0.85f; // 精神状态阈值

    private string techName;

    public override string GetDetails()
    {
        // TODO: 根据是否睡觉调整描述
        return @"麦麦最近精神很好，连脑子都灵光了起来。她突然滔滔不绝地讲起了她对研究的看法。这些的观点十分新颖且具有实际意义，或许比系统里的论文更适合当下情况。
                 总之，这项研究已经没必要在继续了，光靠麦麦就已经全部搞懂了，真不可思议。
                 解锁的科技: " + techName;
    }

    public override bool CanTriggerThisEvent()
    {
        var san = StateManager.Instance.PlayerStateDict[PlayerStateEnum.San];
        return san.CurValue / san.MaxValue >= SAN_THRESHOLD;
    }

    public override void OnTrigger()
    {
        // 立刻完成当前科技
        if (TechnologyManager.Instance.CurStudiedTechNode != null)
        {
            techName = TechnologyManager.Instance.CurStudiedTechNode.techName;
            TechnologyManager.Instance.AddStudyProcess(9999); // 研究进度增加
        }
    }
}
