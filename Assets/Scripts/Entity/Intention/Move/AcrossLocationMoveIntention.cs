using DG.Tweening;
using System.Text;

/// <summary>
/// 跨地点移动意图
/// </summary>
public class AcrossLocationMoveIntention : SingleTargetIntention
{
    protected override bool AutoExecuteOver => false;

    public AcrossLocationMoveIntention(int preparationMinutes, string targetUuid) : base(preparationMinutes, targetUuid)
    {
    }

    public override string GiveName()
    {
        return "移动";
    }

    protected override bool CanExecute()
    {
        // 目标消失或者目标在当前地点
        return EntityTarget != null && !belongedEntity.IsInSameLocation(EntityTarget);
    }

    public override void OnExecute()
    {
        // 跨地点追击
        if (!belongedEntity.ChaseAcrossLocation(EntityTarget, out var tween)  // 追击失败
            || tween == null)                                           // 或追击目标不在玩家所在地点
        {
            ExecuteOver();
            return;
        }

        tween.OnComplete(ExecuteOver);
        tween.OnKill(ExecuteOver);
    }

    public override string GetDescription()
    {
        // 目标是否丢失
        var targetLoss = EntityTarget == null;

        var sb = new StringBuilder();

        // 目标
        if (targetLoss)
            sb.AppendLine($"目标:  已丢失");
        else
            sb.AppendLine($"目标:  {EntityTarget.Name}");

        // 与目标的距离
        if (!targetLoss)
        {
            sb.AppendLine($"目标所在地:  {EntityTarget.Coordinate.Location.PlaceName}");
            sb.AppendLine($"预计到达地点:  {EntityTarget.Coordinate.Location.PlaceName}");
        }

        // TODO: 策划配置的描述文本

        return sb.ToString();
    }
}