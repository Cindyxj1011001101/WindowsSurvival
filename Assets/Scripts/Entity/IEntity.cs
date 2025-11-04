public interface IEntity
{
    string UUID { get; }

    Coordinate Coordinate { get; }

    /// <summary>
    /// 承受伤害
    /// </summary>
    /// <param name="damage">伤害数值</param>
    /// <param name="damageDealer">伤害制造者</param>
    void TakeDamage(float damage, IEntity damageDealer);
}