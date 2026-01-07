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
            sb.AppendLine($"威胁来源:  {ColorManager.Colorize("已丢失", ColorManager.LightGrey)}");
        else
            sb.AppendLine($"威胁来源:  {ColorManager.Colorize(EntityTarget.Name, ColorManager.Yellow)}");

        // 与威胁来源的距离
        if (!threatLoss)
        {
            var dist = belongedEntity.DistanceTo(EntityTarget);
            sb.AppendLine($"威胁来源位置:  {ColorManager.ColorizeNumber(EntityTarget.Coordinate.Position, ColorManager.Cyan)}");
            sb.AppendLine($"威胁来源距离:  {ColorManager.ColorizeNumber(dist, ColorManager.Cyan)}");
            sb.AppendLine($"预计到达位置:  {ColorManager.ColorizeNumber(belongedEntity.EstimateMoveEndPosition(EntityTarget, moveDist, false), ColorManager.Cyan)}");
        }

        // TODO: 策划配置的描述文本

        return sb.ToString();
    }
}