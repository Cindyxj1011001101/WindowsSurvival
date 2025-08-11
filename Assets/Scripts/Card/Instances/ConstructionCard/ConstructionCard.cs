public abstract class ConstructionCard : Card
{
    /// <summary>
    /// 能否放置在当前环境
    /// </summary>
    /// <returns></returns>
    public abstract bool CanPlace(out string hint);
}