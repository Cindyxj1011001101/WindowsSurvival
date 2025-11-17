using Newtonsoft.Json;
using System.Text;

/// <summary>
/// 跨地点移动意图
/// </summary>
public class AcrossLocationMoveIntention : EntityIntention
{
    [JsonProperty] private string targetUuid; // 目标uuid

    public AcrossLocationMoveIntention(int preparationMinutes, string targetUuid) : base(preparationMinutes)
    {
        this.targetUuid = targetUuid;
    }

    public override string GiveName()
    {
        return "长途移动";
    }

    public override bool CanExecute()
    {
        var target = GlobalDataManager.Instance.GetEntityByUuid(targetUuid);
        // 目标消失或者目标在当前地点
        return target == null || belongedEntity.IsInSameLocation(target);
    }

    public override void Execute()
    {
        var target = GlobalDataManager.Instance.GetEntityByUuid(targetUuid);
        // 跨地点追击
        belongedEntity.ChaseAcrossLocation(target);
    }

    public override string GetDescription()
    {
        var target = GlobalDataManager.Instance.GetEntityByUuid(targetUuid);

        // 目标是否丢失
        var targetLoss = target == null;

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
            sb.AppendLine($"目标所在地:  {target.Coordinate.Location.PlaceName}");
            sb.AppendLine($"预计到达地点:  {target.Coordinate.Location.PlaceName}");
        }

        // TODO: 策划配置的描述文本

        return sb.ToString();
    }
}