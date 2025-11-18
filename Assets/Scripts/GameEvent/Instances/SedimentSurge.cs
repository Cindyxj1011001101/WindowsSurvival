using UnityEngine;

/// <summary>
/// 泥沙翻涌
/// </summary>
public class SedimentSurge : GameEvent
{
    private string affectedPlacesStr;

    public override string GetDetails()
    {
        return $"洞穴中的泥沙开始涌动，使得洞穴能见度变得很低。\n\n" +
               $"仅用微弱光照的照明设备无法看清物体，看来只能暂时摸黑了。\n\n" +
               $"受到影响的地点: " + ColorManager.Colorize(affectedPlacesStr, ColorManager.Yellow);
    }

    protected override bool CanTriggerThisEvent()
    {
        return GameManager.Instance.CurEnvironmentBag.PlaceData.isInCave;
    }

    protected override void OnTrigger()
    {
        // 计算威胁事件强度
        var threatIntensity = CalculateThreatIntensity();
        // 计算持续时间
        SetRemainingMinutes(Mathf.CeilToInt((.75f + threatIntensity / 100) * Random.Range(60, 601)));

        affectedPlacesStr = GameManager.Instance.CurEnvironmentBag.PlaceData.placeName;

        // 地点光照-95
        GameManager.Instance.CurEnvironmentBag.SetBrightnessConstValue("泥沙涌动", -95);
    }

    protected override void OnEnd()
    {
        // 移除地点光照-95
        GameManager.Instance.CurEnvironmentBag.SetBrightnessConstValue("泥沙涌动", 0);
    }
}
