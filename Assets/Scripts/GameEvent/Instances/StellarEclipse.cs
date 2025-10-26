/// <summary>
/// 恒星食
/// </summary>
public class StellarEclipse : GameEvent
{
    public override string GetDetails()
    {
        return @"黑色从天空中恒星的一角漫上来，渐渐吞噬了光，恒星食来了。
                 在接下了的数小时甚至数天里将不会有任何恒星光照。";
    }

    public override void OnTrigger()
    {
        
    }
}
