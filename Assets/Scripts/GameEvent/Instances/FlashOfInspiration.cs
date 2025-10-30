/// <summary>
/// 灵光乍现
/// </summary>
public class FlashOfInspiration : GameEvent
{
    private const float SAN_THRESHOLD = 0.85f; // 精神状态阈值

    private string techName;

    public override string GetDetails()
    {
        var desc = $"麦麦最近精神很好，连脑子都灵光了起来。她突然滔滔不绝地讲起了她对研究的看法。" +
                   $"这些的观点十分新颖且具有实际意义，或许比系统里的论文更适合当下情况。\n\n" +
                   $"总之，这项研究已经没必要在继续了，光靠麦麦就已经全部搞懂了，真不可思议。\n\n";

        if (StateManager.Instance.IsResting)
            desc = $"现在进行的研究有好多看不懂的地方，一些提到的东西你完全没听说过。就在这时麦麦说起了几句模糊不清的梦话，奇怪的是这些语句正好解释了论文中你所不懂的地方，你瞬间将问题想通，这项科技你已完全了解。\n\n" +
                   $"真不可思议，她睡着的时候比醒着时聪明多了。\n\n";

        desc += $"解锁的科技: " + ColorManager.Colorize(techName, ColorManager.Cyan);

        return desc;
    }

    protected override bool CanTriggerThisEvent()
    {
        if (TechnologyManager.Instance.CurStudiedTechNode == null) return false;
        var san = StateManager.Instance.PlayerStateDict[PlayerStateEnum.Sanity];
        return san.CurValue / san.MaxValue >= SAN_THRESHOLD;
    }

    protected override void OnTrigger()
    {
        // 立刻完成当前科技
        techName = TechnologyManager.Instance.CurStudiedTechNode.techName;
        TechnologyManager.Instance.AddStudyProcess(9999); // 研究进度增加
    }
}
