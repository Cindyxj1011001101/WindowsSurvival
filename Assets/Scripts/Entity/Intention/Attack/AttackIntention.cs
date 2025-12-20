using Newtonsoft.Json;
using System.Text;
using UnityEngine;

/// <summary>
/// 攻击意图
/// </summary>
public abstract class AttackIntention : EntityIntention
{
    [JsonProperty] protected string targetUuid;       // 攻击目标uuid
    [JsonProperty] protected float dmg;               // 攻击伤害
    [JsonProperty] protected AttackForm atkForm;      // 攻击类型
    [JsonProperty] protected (float, float) atkRange; // 攻击范围

    public AttackIntention(int preparationMinutes, string targetUuid, float dmg, AttackForm atkForm, (float, float) atkRange) : base(preparationMinutes)
    {
        this.targetUuid = targetUuid;
        this.dmg = dmg;
        this.atkForm = atkForm;
        this.atkRange = atkRange;
    }

    protected override bool CanExecute()
    {
        var target = GlobalDataManager.Instance.GetEntityByUuid(targetUuid);

        // 攻击目标丢失
        if (target == null || !belongedEntity.IsInSameLocation(target)) return false;

        // 攻击目标不在距离内
        var dist = belongedEntity.DistanceTo(target);
        if (dist < atkRange.Item1 || dist > atkRange.Item2) return false;

        return true;
    }

    public override void OnExecute()
    {
        var target = GlobalDataManager.Instance.GetEntityByUuid(targetUuid);
        
        // 再次检查执行条件（可能在准备期间状态改变）
        if (!CanExecute())
        {
            // 执行失败，显示失败提示（从实体处弹出）
            ShowExecutionFailedTip(target);
            return;
        }
        
        // 执行攻击
        belongedEntity.SingleAttack(target, dmg);
        
        // TODO: 攻击动效
        
        // TODO: 范围攻击

    }

    /// <summary>
    /// 显示意图执行失败的提示
    /// </summary>
    private void ShowExecutionFailedTip(IEntity target)
    {
        // 优先使用SlotTransform获取实体在背包中的真实位置，不受详情窗口影响
        var entityTransform = belongedEntity.SlotTransform ?? belongedEntity.Transform;
        if (entityTransform == null) return;

        string tip = "执行失败";
        
        if (target == null || !belongedEntity.IsInSameLocation(target))
        {
            tip = "目标已丢失";
        }
        else
        {
            // 检查是否在攻击范围内
            var dist = belongedEntity.DistanceTo(target);
            if (dist < atkRange.Item1)
            {
                tip = "目标过近";
            }
            else if (dist > atkRange.Item2)
            {
                tip = "目标过远";
            }
            else
            {
                tip = "目标不在攻击范围内";
            }
        }

        AnimationManager.Instance.ShowFloatingTipAbove(entityTransform, tip, 0.5f);
    }

    public override string GetDescription()
    {
        var target = GlobalDataManager.Instance.GetEntityByUuid(targetUuid);

        // 攻击目标是否丢失
        var targetLoss = target == null || !belongedEntity.IsInSameLocation(target);

        var sb = new StringBuilder();
        // 攻击伤害
        sb.AppendLine($"攻击伤害:  {dmg:0.0}");

        // 攻击目标
        if (targetLoss)
            sb.AppendLine($"攻击目标:  已丢失");
        else if (target is Player)
            sb.AppendLine($"攻击目标:  麦麦");
        else
            sb.AppendLine($"攻击目标:  {(target as EntityCard).CardName}");

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
            var dist = belongedEntity.DistanceTo(target);
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