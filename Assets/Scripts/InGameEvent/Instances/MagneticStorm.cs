using UnityEngine;

/// <summary>
/// 行星磁暴
/// </summary>
public class MagneticStorm: InGameEvent
{
    public override void TriggerThisEvent()
    {
        // 计算威胁事件强度
        var threatIntensity = CalculateThreatIntensity();
        // 计算持续事件
        var duration = Mathf.CeilToInt((.75f + threatIntensity / 100) * Random.Range(190, 4501));
        // 添加电网故障效果
        GameManager.Instance.AddGlobalEffect(new PowerNetworkFailure(duration));
    }
}
