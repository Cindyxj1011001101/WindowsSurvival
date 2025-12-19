using Newtonsoft.Json;
using System.Text;

/// <summary>
/// 地点内移动
/// </summary>
public class InLocationMoveIntention : EntityIntention
{
    [JsonProperty] private string targetUuid;   // 靠近或远离的目标
    [JsonProperty] private float moveDist;      // 移动距离
    [JsonProperty] private bool moveClose;      // 是否靠近目标移动

    public InLocationMoveIntention(int preparationMinutes, string targetUuid, float moveDist, bool moveClose) : base(preparationMinutes)
    {
        this.targetUuid = targetUuid;
        this.moveDist = moveDist;
        this.moveClose = moveClose;
    }

    public override string GiveName()
    {
        return "移动";
    }

    protected override bool CanExecute()
    {
        var target = GlobalDataManager.Instance.GetEntityByUuid(targetUuid);
        // 目标丢失
        return target != null && belongedEntity.IsInSameLocation(target);
    }

    public override void OnExecute()
    {
        var target = GlobalDataManager.Instance.GetEntityByUuid(targetUuid);

        if (moveClose)
            belongedEntity.MoveTowards(target, moveDist);
        else
            belongedEntity.MoveAwayFrom(target, moveDist);
    }

    public override string GetDescription()
    {
        var target = GlobalDataManager.Instance.GetEntityByUuid(targetUuid);

        // 目标是否丢失
        var targetLoss = target == null || !belongedEntity.IsInSameLocation(target);

        var sb = new StringBuilder();

        // 目标
        if (targetLoss)
            sb.AppendLine($"目标:  已丢失");
        else if (target is Player)
            sb.AppendLine($"目标:  麦麦");
        else
            sb.AppendLine($"目标:  {(target as EntityCard).CardName}");

        // 与目标的距离
        if (!targetLoss)
        {
            var dist = belongedEntity.DistanceTo(target);
            sb.AppendLine($"目标位置:  {target.Coordinate.Position:0.0}");
            sb.AppendLine($"目标距离:  {dist:0.0}");
            sb.AppendLine($"移动方向:  {(moveClose ? "靠近" : "远离")}");
            sb.AppendLine($"预计到达位置:  {belongedEntity.EstimateMoveEndPosition(target, moveDist, moveClose):0.0}");
        }

        // TODO: 策划配置的描述文本

        return sb.ToString();
    }
}