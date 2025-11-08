/// <summary>
/// 鱼汤
/// </summary>
public class FishSoup : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 18 },
                { PlayerStateEnum.Hydration, 33 },
                { PlayerStateEnum.Sanity, 12 },
                { PlayerStateEnum.Health, 20 },
                { PlayerStateEnum.PainLevel, -25 }
            },
            sound: "喝_01");
    }
}