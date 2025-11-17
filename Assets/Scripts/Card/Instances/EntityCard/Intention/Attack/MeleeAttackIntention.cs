/// <summary>
/// 近战攻击意图
/// </summary>
public class MeleeAttackIntention : AttackIntention
{
    public MeleeAttackIntention(int preparationMinutes, string targetUuid, float dmg, AttackForm atkForm, (float, float) atkRange) : base(preparationMinutes, targetUuid, dmg, atkForm, atkRange)
    {
    }
}