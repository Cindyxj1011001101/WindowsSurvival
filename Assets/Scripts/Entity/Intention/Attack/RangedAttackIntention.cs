/// <summary>
/// 远程攻击意图
/// </summary>
public class RangedAttackIntention : AttackIntention
{
    public RangedAttackIntention(int preparationMinutes, string targetUuid, float dmg, AttackForm atkForm, (float, float) atkRange) : base(preparationMinutes, targetUuid, dmg, atkForm, atkRange)
    {
    }

    public override string GiveName()
    {
        return "远程攻击";
    }
}