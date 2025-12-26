/// <summary>
/// 近战攻击意图
/// </summary>
public class MeleeAttackIntention : AttackIntention
{
    protected override bool WithoutAnim => false;

    public MeleeAttackIntention(int preparationMinutes, string targetUuid, float dmg, AttackForm atkForm, (float, float) atkRange) : base(preparationMinutes, targetUuid, dmg, atkForm, atkRange)
    {
    }

    public override string GiveName()
    {
        return "近战攻击";
    }

    public override void OnExecute()
    {
        var sourceSlot = belongedEntity.Slot;
        var tempSlot = AnimationManager.Instance.CreateSlotCopy(belongedEntity);

        if (tempSlot == null)
        {
            PerformAttack();
            OnComplete();
            return;
        }

        if (EntityTarget is Player)
        {
            AnimationManager.Instance.PlayMeleeAttackPlayerEffect(tempSlot, PerformAttack, OnComplete);
        }
        else
        {
            var targetPos = (EntityTarget as Card).SlotTransform.position;
            AnimationManager.Instance.PlayMeleeAttackToPositionEffect(
                tempSlot,
                targetPos,
                PerformAttack,
                OnComplete);
        }

        void OnComplete()
        {
            if (sourceSlot != null)
                sourceSlot.DontRefresh = false;

            ExecuteOver();
        }
    }
}