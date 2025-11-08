/// <summary>
/// 腐烂物
/// </summary>
public class SpoiledFood : CookableCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "吃这个？你疯了", EasyEvent_Destroy, null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 6 },
                { PlayerStateEnum.Sanity, -20 },
                { PlayerStateEnum.Health, -10 }
            },
            sound: "吃_01");
    }
}