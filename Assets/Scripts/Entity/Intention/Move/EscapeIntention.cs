using Newtonsoft.Json;
using System.Text;

/// <summary>
/// 逃跑意图
/// </summary>
public class EscapeIntention : EntityIntention
{
    [JsonProperty] private string targetUuid;   // 远离的目标
    [JsonProperty] private float escapeDist;    // 逃跑距离

    public EscapeIntention(int preparationMinutes, string targetUuid, float escapeDist) : base(preparationMinutes)
    {
        this.targetUuid = targetUuid;
        this.escapeDist = escapeDist;
    }

    public override string GiveName()
    {
        return "逃跑";
    }

    protected override bool CanExecute()
    {
        var threat = GlobalDataManager.Instance.GetEntityByUuid(targetUuid);
        // 威胁丢失
        return threat != null && belongedEntity.IsInSameLocation(threat);
    }

    public override void OnExecute()
    {
        var threat = GlobalDataManager.Instance.GetEntityByUuid(targetUuid);
        // 向远离威胁来源的距离逃跑
        belongedEntity.EscapeFrom(threat, escapeDist);
    }

    public override string GetDescription()
    {
        var threat = GlobalDataManager.Instance.GetEntityByUuid(targetUuid);

        // 威胁是否丢失
        var threatLoss = threat == null || !belongedEntity.IsInSameLocation(threat);

        var sb = new StringBuilder();

        // 威胁来源
        if (threatLoss)
            sb.AppendLine($"威胁来源:  已丢失");
        else if (threat is Player)
            sb.AppendLine($"威胁来源:  麦麦");
        else
            sb.AppendLine($"威胁来源:  {(threat as EntityCard).CardName}");

        // 与威胁来源的距离
        if (!threatLoss)
        {
            var dist = belongedEntity.DistanceTo(threat);
            sb.AppendLine($"威胁来源位置:  {threat.Coordinate.Location:0.0}");
            sb.AppendLine($"威胁来源距离:  {dist:0.0}");
            sb.AppendLine($"预计到达位置:  {belongedEntity.EstimateMoveEndPosition(threat, escapeDist, false):0.0}");
        }

        // TODO: 策划配置的描述文本

        return sb.ToString();
    }
}