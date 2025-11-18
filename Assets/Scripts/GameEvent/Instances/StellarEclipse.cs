using UnityEngine;

/// <summary>
/// 恒星食
/// </summary>
public class StellarEclipse : GameEvent
{
    public override string GetDetails()
    {
        return $"黑色从天空中恒星的一角漫上来，渐渐吞噬了光，恒星食来了。\n\n" +
               $"在接下了的数小时甚至数天里将不会有任何恒星光照。";
    }

    protected override void OnTrigger()
    {
        // 计算威胁事件强度
        var threatIntensity = CalculateThreatIntensity();
        // 计算持续时间
        SetRemainingMinutes(Mathf.CeilToInt((.75f + threatIntensity / 100) * Random.Range(100, 3601)));
    }
}
