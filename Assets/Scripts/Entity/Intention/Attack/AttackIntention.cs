using Newtonsoft.Json;
using System.Text;

/// <summary>
/// 攻击意图
/// </summary>
public abstract class AttackIntention : SingleTargetIntention
{
    [JsonProperty] protected float dmg;               // 攻击伤害
    [JsonProperty] protected AttackForm atkForm;      // 攻击类型
    [JsonProperty] protected (float, float) atkRange; // 攻击范围

    public AttackIntention(int preparationMinutes, string targetUuid, float dmg, AttackForm atkForm, (float, float) atkRange) : base(preparationMinutes, targetUuid)
    {
        this.dmg = dmg;
        this.atkForm = atkForm;
        this.atkRange = atkRange;
    }

    protected override bool CanExecute()
    {
        // 攻击目标丢失
        if (EntityTarget == null || !belongedEntity.IsInSameLocation(EntityTarget)) return false;

        // 攻击目标不在距离内
        var dist = belongedEntity.DistanceTo(EntityTarget);
        if (dist < atkRange.Item1 || dist > atkRange.Item2) return false;

        return true;
    }

    public override void OnExecute()
    {
        // 执行攻击
        PerformAttack();

        // TODO: 攻击动效
        
        // TODO: 范围攻击

    }

    /// <summary>
    /// 执行攻击
    /// </summary>
    protected virtual void PerformAttack()
    {
        belongedEntity.SingleAttack(EntityTarget, dmg);
    }

    public override string GetDescription()
    {
        // 攻击目标是否丢失
        var targetLoss = EntityTarget == null || !belongedEntity.IsInSameLocation(EntityTarget);

        var sb = new StringBuilder();
        // 攻击伤害
        sb.AppendLine($"攻击伤害:  {dmg:0.0}");

        // 攻击目标
        if (targetLoss)
            sb.AppendLine($"攻击目标:  已丢失");
        else
            sb.AppendLine($"攻击目标:  {EntityTarget.Name}");

        // 攻击类型
        switch (atkForm)
        {
            case AttackForm.Single:
                sb.AppendLine($"攻击类型:  单体");
                break;
            case AttackForm.AOE:
                sb.AppendLine($"攻击类型:  群体");
                break;
        }
        
        // 攻击距离
        sb.AppendLine($"攻击距离:  [{atkRange.Item1}, {atkRange.Item2}]");

        // 能否攻击到
        if (!targetLoss)
        {
            var dist = belongedEntity.DistanceTo(EntityTarget);
            sb.AppendLine($"目标距离:  {dist:0.0}");

            if (dist >= atkRange.Item1 && dist <= atkRange.Item2)
                sb.AppendLine($"目标在攻击距离内:  是");
            else
                sb.AppendLine($"目标在攻击距离内:  否");
        }

        // TODO: 策划配置的描述文本

        return sb.ToString();
    }
}