using DG.Tweening;
using Newtonsoft.Json;
using System.Text;

/// <summary>
/// 地点内移动
/// </summary>
public class InLocationMoveIntention : SingleTargetIntention
{
    [JsonProperty] protected float moveDist;      // 移动距离
    [JsonProperty] protected bool moveClose;      // 是否靠近目标移动
    [JsonProperty] protected bool escape;         // 是否逃跑

    protected override bool AutoExecuteOver => false;

    public InLocationMoveIntention(int preparationMinutes, string targetUuid, float moveDist, bool moveClose) : base(preparationMinutes, targetUuid)
    {
        this.moveDist = moveDist;
        this.moveClose = moveClose;
    }

    public override string GiveName()
    {
        return "移动";
    }

    protected override bool CanExecute()
    {
        // 目标丢失
        return EntityTarget != null && belongedEntity.IsInSameLocation(EntityTarget);
    }

    public override void OnExecute()
    {
        var sourceSlot = belongedEntity.Slot;
        CardSlot tempSlot = null;
        if (sourceSlot != null)
        {
            tempSlot = AnimationManager.Instance.CreateTempSlot(sourceSlot.transform.position);
            sourceSlot.Clear();
            sourceSlot.DontRefresh = true;
            tempSlot.DisplayCard(belongedEntity, 1, false);
        }

        if (moveClose)
            belongedEntity.MoveTowards(EntityTarget, moveDist);
        else
            belongedEntity.MoveAwayFrom(EntityTarget, moveDist);

        if (tempSlot != null)
        {
            AnimationManager.Instance.PlayMoveIntentionEffect(belongedEntity, tempSlot, () =>
            {
                sourceSlot.DontRefresh = false;
                if (escape)
                    belongedEntity.TryEscape();
                ExecuteOver();
            });
        }
        else
        {
            if (escape)
                belongedEntity.TryEscape();
            ExecuteOver();
        }
    }

    public override string GetDescription()
    {
        // 目标是否丢失
        var targetLoss = EntityTarget == null || !belongedEntity.IsInSameLocation(EntityTarget);

        var sb = new StringBuilder();

        // 目标
        if (targetLoss)
            sb.AppendLine($"目标:  已丢失");
        else
            sb.AppendLine($"目标:  {EntityTarget.Name}");

        // 与目标的距离
        if (!targetLoss)
        {
            var dist = belongedEntity.DistanceTo(EntityTarget);
            sb.AppendLine($"目标位置:  {EntityTarget.Coordinate.Position:0.0}");
            sb.AppendLine($"目标距离:  {dist:0.0}");
            sb.AppendLine($"移动方向:  {(moveClose ? "靠近" : "远离")}");
            sb.AppendLine($"预计到达位置:  {belongedEntity.EstimateMoveEndPosition(EntityTarget, moveDist, moveClose):0.0}");
        }

        // TODO: 策划配置的描述文本

        return sb.ToString();
    }
}