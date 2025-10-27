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

    public override void OnTrigger()
    {
        
    }
}
