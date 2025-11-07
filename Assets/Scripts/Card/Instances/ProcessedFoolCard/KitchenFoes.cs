/// <summary>
/// 厨房恶物
/// </summary>
public class KitchenFoes : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 13 },
                { PlayerStateEnum.Sanity, -6 },
                { PlayerStateEnum.Health, -4 }
            });
    }
}