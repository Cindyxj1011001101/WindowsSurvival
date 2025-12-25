using DG.Tweening;
using System.Text;

/// <summary>
/// 逃跑意图
/// </summary>
public class EscapeIntention : InLocationMoveIntention
{
    public EscapeIntention(int preparationMinutes, string targetUuid, float escapeDist) : base(preparationMinutes, targetUuid, escapeDist, false)
    {
        this.escape = true;
    }

    public override string GiveName()
    {
        return "逃跑";
    }

    public override string GetDescription()
    {
        // 威胁是否丢失
        var threatLoss = EntityTarget == null || !belongedEntity.IsInSameLocation(EntityTarget);

        var sb = new StringBuilder();

        // 威胁来源
        if (threatLoss)
            sb.AppendLine($"威胁来源:  已丢失");
        else
            sb.AppendLine($"威胁来源:  {EntityTarget.Name}");

        // 与威胁来源的距离
        if (!threatLoss)
        {
            var dist = belongedEntity.DistanceTo(EntityTarget);
            sb.AppendLine($"威胁来源位置:  {EntityTarget.Coordinate.Position:0.0}");
            sb.AppendLine($"威胁来源距离:  {dist:0.0}");
            sb.AppendLine($"预计到达位置:  {belongedEntity.EstimateMoveEndPosition(EntityTarget, moveDist, false):0.0}");
        }

        // TODO: 策划配置的描述文本

        return sb.ToString();
    }
}