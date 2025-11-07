/// <summary>
/// 恶臭肉
/// </summary>
public class FoulSmellingMeat : CookableCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 14 },
                { PlayerStateEnum.Sanity, -20 },
                { PlayerStateEnum.Health, -15 }
            });
    }
}
