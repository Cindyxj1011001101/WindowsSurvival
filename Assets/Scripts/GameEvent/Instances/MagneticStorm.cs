using UnityEngine;

/// <summary>
/// 行星磁暴
/// </summary>
public class MagneticStorm : GameEvent
{
    public override string GetDetails()
    {
        return $"所有的电器突然停止了运作，麦麦说可能是因为行星磁暴。\n\n" +
               $"总之，接下来的一段时间里，所有电器都无法使用了。但愿磁暴不会持续太久。";
    }

    public override void OnTrigger()
    {
        // 计算威胁事件强度
        var threatIntensity = CalculateThreatIntensity();
        // 计算持续时间
        remainingMinutes = Mathf.CeilToInt((.75f + threatIntensity / 100) * Random.Range(190, 4501));
    }
}
