using UnityEngine;

/// <summary>
/// 移动激励
/// </summary>
public class MovementIncentive : GameEvent
{
    private const float SAN_THRESHOLD = 0.85f; // 精神状态阈值

    public override string GetDetails()
    {
        return $"麦麦最近精神很好，连游泳和跑步都变快了不少。\n\n" +
               $"在接下来的一段时间里，麦麦{ColorManager.Colorize("-50%", ColorManager.Green)}移动时长。";
    }

    protected override bool CanTriggerThisEvent()
    {
        var san = StateManager.Instance.PlayerStateDict[PlayerStateEnum.Sanity];
        return san.CurValue / san.MaxValue >= SAN_THRESHOLD;
    }

    protected override void OnTrigger()
    {
        SetRemainingMinutes(Random.Range(120, 1441));
        MoveExploreManager.Instance.AddMoveExtraEffect("移动激励", -0.5f, null);
    }

    protected override void OnEnd()
    {
        MoveExploreManager.Instance.RemoveMoveExtraEffect("移动激励");
    }
}
