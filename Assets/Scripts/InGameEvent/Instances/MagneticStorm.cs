using UnityEngine;

/// <summary>
/// 行星磁暴
/// </summary>
public class MagneticStorm : GameEvent
{
    public override void OnTrigger()
    {
        // 计算威胁事件强度
        var threatIntensity = CalculateThreatIntensity();
        // 计算持续事件
        remainingTime = Mathf.CeilToInt((.75f + threatIntensity / 100) * Random.Range(190, 4501));
    }
}
