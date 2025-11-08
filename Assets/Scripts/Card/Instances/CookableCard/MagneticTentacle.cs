/// <summary>
/// 磁性触手
/// </summary>
public class MagneticTentacle : CookableCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "闻起来有铁锈味", EasyEvent_Destroy, null,
            () => 30,
            () => new()
            {
                { PlayerStateEnum.Hunger, 14 },
                { PlayerStateEnum.Sanity, -6 },
                { PlayerStateEnum.Health, -5 }
            },
            sound: "吃_01");
    }
}